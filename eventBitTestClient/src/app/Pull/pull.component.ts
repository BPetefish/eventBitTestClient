//Modules
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Http, Response, Headers } from '@angular/http';
import { DatePipe } from '@angular/common'
import { ToasterContainerComponent, ToasterService, ToasterConfig } from 'angular2-toaster';
import { ActivatedRoute } from '@angular/router';

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

            //I'm going to set my search criteria so I can reassign it on a back.
            let sc:any = JSON.parse(localStorage.getItem("SEARCHC"));
            if (sc)
            {
                if (sc.showCode)
                    this.showCode = sc.showCode;

                if (sc.entityId)
                    this.entSync = sc.entityId;

                localStorage.setItem("SEARCHC", null);
            }

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
                this.logText("Request: " + resp.LastSince + " | Processed: " + resp.Count + " rows.");

                if (resp.Count > 0)
                    this.syncEntityLoop(entityId)

            } else if (resp && resp.Count == 0) {

                this.logText("Request: " + resp.LastSince + " | Processed: " + resp.Count + " rows.  Entity is currently up to date.");

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

    copyTextArea() {
        var copyTextarea = document.querySelector('.js-copytextarea');
        (<HTMLTextAreaElement>copyTextarea).select();

        try {
            var successful = document.execCommand('copy');
            this.toastr.pop('success', 'Text copied to clipboard.');
        } catch (err) {
            this.toastr.pop('warning', 'Unable to copy text to clipboard.');
        }
    }

    previewEntity() {

        if (!this.showCode) {
            //Test
            this.toastr.pop('error', 'You must provide a show code.');
            return;
        }

        if (!this.entSync) {
            this.toastr.pop('error', 'You select an entity to preview.');
            return;
        }

        var searchC:any = {};
        searchC.showCode = this.showCode;
        searchC.entityId = this.entSync;

        //I'm going to set my search criteria so I can reassign it on a back.
        localStorage.setItem("SEARCHC", JSON.stringify(searchC));

        this.router.navigateByUrl('/preview/' + this.showCode + '/' + this.entSync);
    }
}