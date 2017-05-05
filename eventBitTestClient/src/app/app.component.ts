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

    constructor(private router: Router, private http: Http) { }

    model = new loginDTO('', '');

    login() {
        var headers = new Headers();
        headers.append('Content-Type', 'application/json');
        this.http
            .post('https://dev.experienteventbit.com/webapi/API/AuthUser',
            JSON.stringify(this.model), {
                headers: headers
            })
            .subscribe(data => {

                var headers = data.headers;
                var body = JSON.parse(data.text());
                var d = new LoginSessionData(headers, body);
                localStorage.setItem('Login', JSON.stringify(d));
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

    constructor() { this.activate() }   

    activate() {

        var loginInfo = JSON.parse(localStorage.getItem('Login'));

        this.loggedUser.FirstName = loginInfo.Body.FirstName;
        this.loggedUser.LastName = loginInfo.Body.LastName;
        this.loggedUser.Email = loginInfo.Body.Email;
        debugger;

    }

}
