//Modules
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Http, Response, Headers } from '@angular/http';
import { DatePipe } from '@angular/common'
import { ToasterContainerComponent, ToasterService, ToasterConfig } from 'angular2-toaster';

//Classes
import { User } from './Classes/User'

@Component({
    selector: 'app-pull',
    templateUrl: 'App/Pull/Views/Pull.html'
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

    constructor(private http: Http, private toastr: ToasterService, private datePipe: DatePipe, private router: Router) {

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
            this.entSync = "";
            this.loading = false;

        }, error => {
            alert('Error');
        });
    }

    syncEntities() {
        this.syncEntityLoop(this.entSync);
    }

    syncEntityLoop(entityId: string) {

        if (!this.showCode) {
            //Test
            this.toastr.pop('error', 'You must provide a show code.');
            return;
        }

        if (!entityId) {
            this.toastr.pop('error', 'You select an entity to sync.');
            return;
        }

        this.saving = true;

        this.logText("Sending request to sync '" + entityId + "'");

        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        //Check for timeout and such.
        if (!this.IsHeaderValid(claim))
            return;

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

            if (resp && resp.Count) {
                this.logText("Processed " + resp.Count + " rows for entity '" + entityId + "'");

                if (resp.Count > 0)
                    this.syncEntityLoop(entityId)

            } else if (resp && resp.Count == 0) {
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
            this.toastr.pop('error', 'You must provide a show code.');
            return;
        }

        this.snapshotSaving = true;

        this.logText("Snapshot request for event: " + this.showCode);

        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        if (!this.IsHeaderValid(claim))
            return;

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

        var d = this.datePipe.transform(new Date(), 'MM/dd/y hh:mm:ss a');

        this.log.push(d + ": " + text);

        var revLog = this.log.slice();

        this.taText = revLog.reverse().join("\n");

    }

    clearLog() {

        this.log = [];
        this.taText = "";
    }

    IsHeaderValid(claim: string) {
        var header = JSON.parse(claim);

        if (header.Expires && (header.Expires - (new Date).getTime()) <= 0) {
            this.router.navigateByUrl('/');
            this.toastr.pop('error', 'Your session has timed out');
            return false;
        }

        return true;
    }
}