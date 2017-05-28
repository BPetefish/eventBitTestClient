import { Component} from '@angular/core';


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




