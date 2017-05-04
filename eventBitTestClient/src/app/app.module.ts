import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';

import { PullComponent } from './app.component';
import { LoginComponent } from './app.component';

//Import Router
import { Routes, RouterModule } from '@angular/router';

export const routes: Routes = [
    { path: '', component: LoginComponent },
    { path: 'pull', component: PullComponent }
];

@NgModule({
    imports: [BrowserModule, RouterModule.forRoot(routes), FormsModule, HttpModule],
    declarations: [AppComponent, PullComponent, LoginComponent],
    bootstrap: [AppComponent]
})
export class AppModule { }
