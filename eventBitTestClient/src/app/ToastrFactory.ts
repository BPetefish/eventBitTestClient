import { ToasterContainerComponent, ToasterService, ToasterConfig } from 'angular2-toaster';

export class ToastrFactory {

    private toastr:ToasterService = new ToasterService();

    constructor() { }

    popSuccess(body: string, title?:string) {
        this.toastr.pop('success', title, body);
    }
}