import { Component} from '@angular/core';
import { Router } from '@angular/router';
import { Http, Response, Headers } from '@angular/http';

import { DatePipe } from '@angular/common'

import { loginDTO } from './Login/loginDTO';
import { LoginSessionData } from './Login/LoginSessionData'
import { User } from './Pull/User'

//Toastr Class
import { ToasterContainerComponent, ToasterService, ToasterConfig } from 'angular2-toaster';

@Component({
    selector: 'my-app',
    template: `
    <toaster-container></toaster-container>
    <div class="outer-outlet">
      <router-outlet></router-outlet>        
    </div>
  `
})
export class AppComponent {}

@Component({
    selector: 'app-login',
    templateUrl: 'App/Login/Login.html',
    styleUrls: ['App/Styles/login.css'],
})
export class LoginComponent {

    public model: loginDTO;
    public remMe: boolean;
    public logging: boolean;

    constructor(private router: Router, private http: Http, private toastr: ToasterService) {
        this.model = new loginDTO();
        this.remMe = false;
        this.logging = false;

        if (document.cookie)
        {
            var cookies = document.cookie.split(';');

            var userName = cookies[0].split('=')[1];

            this.model.Username = userName;
            this.remMe = true;
        }

    }

    login() {

        if (!this.model.Username || !this.model.Password)
        {
            this.toastr.pop('error', 'Username and Password are required.');
            return;
        }

        if (this.remMe) {

            var now = new Date();
            var expDate = new Date();
            expDate.setDate(now.getDate() + 120);

            document.cookie = "username=" + this.model.Username + "; expires=" + expDate.toUTCString();
        } else {
            document.cookie = "username=; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
        }


        this.logging = true;
        var headers = new Headers();
        headers.append('Content-Type', 'application/json');
        this.http
            .post('https://dev.experienteventbit.com/webapi/API/AuthUser',
            JSON.stringify(this.model), {
                headers: headers
            })
            .subscribe(data => {

                var claim = data.headers.get('X-AUTH-CLAIMS');

                localStorage.setItem('X-AUTH-CLAIMS', claim);

                localStorage.setItem('Login', data.text());
                //Success call means logged in
                this.router.navigateByUrl('/pull');
                this.logging = false;


            }, error => {
                var error = JSON.parse(error.text());
                for (let e of error) {
                    if (e.Text)
                        this.toastr.pop('error', e.Text);
                }

                this.logging = false;
            });
    }
}

@Component({
    selector: 'app-pull',
    templateUrl: 'App/Pull/Views/Pull.html',
    styleUrls: ['App/Styles/pull.css'],
})
export class PullComponent {

    public loggedUser: User;
    public entSync: string;
    public saving: boolean;
    public snapshotSaving: boolean;
    public ent: string[];
    public showCode: string;
    private log: string[];
    public taText: string;
    public loading: boolean;

    constructor(private http: Http, private toastr: ToasterService, private datePipe: DatePipe) {

        this.getEntities();

        var loginInfo = JSON.parse(localStorage.getItem('Login'));

        this.log = [];
        this.loggedUser = new User();

        this.loggedUser.FirstName = loginInfo.FirstName;
        this.loggedUser.LastName = loginInfo.LastName;
        this.loggedUser.Email = loginInfo.Email;

        this.loading = true;
    }

    getEntities() {
        this.http.get('/api/Sync/').subscribe(data => {

            this.ent = JSON.parse(data.text());
            this.loading = false;

        }, error => {
            alert('Error');
        });
    }

    syncEntities() {       
        this.syncEntityLoop(this.entSync);
    }

    syncEntityLoop(entityId: string) {

        if (!this.showCode)
        {
            this.toastr.pop('error', 'You provide a show code.');
            return;
        }

        if (!entityId) {
            this.toastr.pop('error', 'You select an entity to sync.');
            return;
        }

        this.saving = true;

        this.logText("Sending request to sync '" + entityId + "'");

        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        var headers = new Headers();
        headers.append('X-AUTH-CLAIMS', claim);

        this.http.get('/api/Sync/' + entityId + '/' + this.showCode, {
            headers: headers
        }).subscribe(data => {
            //debugger;
            var claim = data.headers.get('X-AUTH-CLAIMS');

            localStorage.setItem('X-AUTH-CLAIMS', claim);

            this.saving = false;

            var resp = JSON.parse(data.text());

            if (resp && resp.Count)
            {
                this.logText("Processed " + resp.Count + " rows for entity '" + entityId + "'");

                if (resp.Count > 0)
                    this.syncEntityLoop(entityId)              
                 
            } else if (resp && resp.Count == 0)
            {
                //this.logText("Processed " + resp.count + " rows for entity '" + entityId + "'");
                this.logText("Received back zero rows for request. '" + entityId + "' currently up to date.")

                this.toastr.pop('success', entityId + ' received zero entires back with latest since stamp, sync is complete.')
            }

        }, error => {
            this.saving = false;
            var errors = JSON.parse(error.text());

            for (let e of errors) {
                if (e.Text) {
                    this.toastr.pop('error', e.Text);
                    this.logText(e.Text);
                }
            }
        });
    }

    pullSnapShot() {

        if (!this.showCode) {
            this.toastr.pop('error', 'You provide a show code.');
            return;
        }

        this.snapshotSaving = true;

        this.logText("Snapshot request for event: " + this.showCode);

        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        var headers = new Headers();
        headers.append('X-AUTH-CLAIMS', claim);

        this.http.get('/api/Snapshot/' + this.showCode, {
            headers: headers
        }).subscribe(data => {

            this.toastr.pop('success', data.text());
            this.logText(data.text());

            var claim = data.headers.get('X-AUTH-CLAIMS');
            localStorage.setItem('X-AUTH-CLAIMS', claim);

            this.snapshotSaving = false;
        }, error => {

            var errors = JSON.parse(error.text());

            for (let e of errors) {
                if (e.Text) {
                    this.toastr.pop('error', e.Text);
                    this.logText(e.Text);
                }
            }
                       
            this.snapshotSaving = false;
        });
    }

    logText(text: string) {

        var d = this.datePipe.transform(new Date(), 'MM/dd/y hh:mm:ss a')

        this.log.push(d + ": " + text);

        var revLog = this.log.slice();

        this.taText = revLog.reverse().join("\n");

    }

    clearLog() {

        this.log = [];
        this.taText = "";
    }

    copyLog() {
      //  var ca = document.querySelector('.copyArea');

      //  ca.select()
    }
}



