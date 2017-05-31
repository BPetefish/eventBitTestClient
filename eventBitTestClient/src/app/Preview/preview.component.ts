//Modules
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http, Response, Headers } from '@angular/http';
import { Router } from '@angular/router';
//Classes

@Component({
    selector: 'app-preview',
    templateUrl: 'App/Preview/Views/Preview.html',
})
export class PreviewComponent {    

    showCode: string;
    entityId: string;
    columns: any;
    tblData: any;
    sysRowStampNumMax: string;
    rowCount: number;
    loading: boolean = true;

    constructor(private route: ActivatedRoute, private http: Http, private router: Router) {    
        this.route.params.subscribe(params => {
            this.showCode = params["showCode"];
            this.entityId = params["entityId"];
        });

        this.http.get('/api/Preview/' + this.showCode + '/' + this.entityId).subscribe(data => {
            var d = JSON.parse(data.text());
            this.columns = d.columns;
            this.tblData = d.data;
            this.sysRowStampNumMax = d.sysRowStampNumMax;
            this.rowCount = d.rowCount;
            this.loading = false;

        }, error => {
            alert('Error');
            this.loading = false;
        });
    } 

    back() {
        this.router.navigateByUrl('/pull')
    }
    
}