import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Http, Response, Headers } from '@angular/http';

import { loginDTO } from './Login/loginDTO';
import { LoginSessionData } from './Login/LoginSessionData'
import { User } from './Pull/User'
//@Component({
//  selector: 'my-app',
//  templateUrl: 'App/Login/Views/Login.html',
//  styleUrls: ['App/Styles/login.css'],
//})
//export class AppComponent  { }

@Component({
    selector: 'my-app',
    template: `
    <div class="outer-outlet">
      <router-outlet></router-outlet>
    </div>
  `
})
export class AppComponent { }

@Component({
    selector: 'app-login',
    templateUrl: 'App/Login/Login.html',
    styleUrls: ['App/Styles/login.css'],
})
export class LoginComponent {

    public model: loginDTO;
    public remMe: boolean;

    constructor(private router: Router, private http: Http) {
        this.model = new loginDTO();
        this.remMe = false;
    }

    

    login() {

        if (this.remMe) {
            document.cookie = "username=" + this.model.Username + "; expires=" + new Date() + 120;
        }

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



            }, error => {
                //toastr.warning('My name is Inigo Montoya. You killed my father, prepare to die!');
            });
        // this.router.navigateByUrl('/pull');
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

    constructor(private http: Http) { this.activate() }

    ent = ["Beacon", "Booth", "Category", "Company", "CompanyAltName", "CompanyBooth", "CompanyCategory", "Facility", "FieldDetail", "FieldDetailPick",
        "Location", "LocationProduct", "LocationSchedule", "Map", "MapBooth", "Person", "PersonCategory", "PersonCompany", "PersonFieldDetailPick", "PersonPurchase",
        "PersonRegistration", "PersonReservation", "Product", "ProductCategory"];     

    activate() {

        var loginInfo = JSON.parse(localStorage.getItem('Login'));
        

        this.loggedUser = new User();

        this.loggedUser.FirstName = loginInfo.FirstName;
        this.loggedUser.LastName = loginInfo.LastName;
        this.loggedUser.Email = loginInfo.Email;

        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        var headers = new Headers();
        headers.append('X-AUTH-CLAIMS', claim);

        this.http.get('/api/Pull', {
            headers: headers
        }).subscribe(data => {
            localStorage.setItem('X-AUTH-CLAIMS', data.text());
        }, error => {
            
        });
        //debugger;

    }

    syncEntities() {

        this.saving = true;
        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        var headers = new Headers();
        headers.append('X-AUTH-CLAIMS', claim);

        this.http.get('/api/Sync/' + this.entSync, {
            headers: headers
        }).subscribe(data => {
            localStorage.setItem('X-AUTH-CLAIMS', data.text());
            this.saving = false;
        }, error => {

        });
    }

    pullSnapShot() {

        var claim = localStorage.getItem('X-AUTH-CLAIMS');

        var headers = new Headers();
        headers.append('X-AUTH-CLAIMS', claim);

        this.http.get('/api/Pull', {
            headers: headers
        }).subscribe(data => {
            localStorage.setItem('X-AUTH-CLAIMS', data.text());
        }, error => {

        });
    }

}
