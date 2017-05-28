import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';

import { PullComponent } from './Pull/pull.component';
import { LoginComponent } from './Login/login.component';
import { ToasterModule } from 'angular2-toaster';
//Import Router
import { Routes, RouterModule } from '@angular/router';


import { DatePipe } from '@angular/common'


export const routes: Routes = [
    { path: '', component: LoginComponent },
    { path: 'pull', component: PullComponent }
];

@NgModule({
    imports: [BrowserModule, RouterModule.forRoot(routes), FormsModule, HttpModule, ToasterModule],
    declarations: [AppComponent, PullComponent, LoginComponent],
    bootstrap: [AppComponent],
    providers: [DatePipe]
})
export class AppModule { }
