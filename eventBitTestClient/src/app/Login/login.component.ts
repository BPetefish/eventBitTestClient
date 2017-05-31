//Modules
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Http, Response, Headers } from '@angular/http';
import { ToasterContainerComponent, ToasterService, ToasterConfig } from 'angular2-toaster';

//Classes
import { loginDTO } from './Classes/loginDTO';
import { LoginSessionData } from './Classes/LoginSessionData'

@Component({
    selector: 'app-login',
    templateUrl: 'App/Login/Views/Login.html',
})
export class LoginComponent {

    public model: loginDTO;
    public remMe: boolean;
    public logging: boolean;

    constructor(private router: Router, private http: Http, private toastr: ToasterService) {
        this.model = new loginDTO();
        this.remMe = false;
        this.logging = false;

        if (document.cookie) {
            var cookies = document.cookie.split(';');

            var userName = cookies[0].split('=')[1];

            this.model.Username = userName;
            this.remMe = true;
        }

    }

    //Login the user and get the x-auth-token
    login() {

        if (!this.model.Username || !this.model.Password) {
            this.toastr.pop('error', 'Username and Password are required.');
            return;
        }

        //Remember Me Checkbox Logic
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