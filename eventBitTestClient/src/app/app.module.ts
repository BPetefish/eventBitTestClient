//Imports
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { ToasterModule } from 'angular2-toaster';
import { Routes, RouterModule } from '@angular/router';

//Providers
import { DatePipe } from '@angular/common'

//Components
import { AppComponent } from './app.component';
import { PullComponent } from './Pull/pull.component';
import { LoginComponent } from './Login/login.component';
import { PreviewComponent } from './Preview/preview.component';


export const routes: Routes = [
    { path: '', component: LoginComponent },
    { path: 'pull', component: PullComponent },
    { path: 'preview/:showCode/:entityId', component: PreviewComponent }
];

@NgModule({
    imports: [BrowserModule, RouterModule.forRoot(routes), FormsModule, HttpModule, ToasterModule],
    declarations: [AppComponent, PullComponent, LoginComponent, PreviewComponent],
    bootstrap: [AppComponent],
    providers: [DatePipe]
})
export class AppModule { }
