import { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbEntityActionArgs, UmbEntityActionBase } from "@umbraco-cms/backoffice/entity-action";
//import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from "@umbraco-cms/backoffice/notification";
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalManagerContext } from "@umbraco-cms/backoffice/modal";
import { MEMBER_CUSTOM_MODAL } from "../../modal/modal-token.ts";

export class MemberEntityAction extends UmbEntityActionBase<never> {
    //#notificationContext? : UmbNotificationContext;
    #modalManagerContext?: UmbModalManagerContext;
    constructor(host: UmbControllerHostElement, args: UmbEntityActionArgs<never>)
    {
        super(host, args)
                // Fetch/consume the contexts & assign to the private fields
        this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
            this.#modalManagerContext = instance;
        });
        //this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
        //    this.#notificationContext = instance;
        //});
    }
    
    async execute() {
                //The modal does NOT return any data when closed (it does not submit)
        const modal = this.#modalManagerContext?.open(this, MEMBER_CUSTOM_MODAL, {
            data: {
                headline:'Resend Validation',
                content: 'Do you want to resend the validation Email?'
            }
        });

        await modal?.onSubmit().then(() => {
            const headers: Headers = new Headers()
            headers.set('Content-Type', 'application/json')
            headers.set('Accept', 'application/json')
            //339fe5d9-3136-42e1-b10f-f3c76bedc315
            const request: RequestInfo = new Request('/sendvalidation', {
                method: 'GET',
                headers: headers,
            })
            // Send the request and print the response
            return fetch(request)
            .then(res => {
                console.log("got response:", res)
            })
        }).catch(() => {
            // User clicked close/cancel and no data was submitted
            console.log('rejected:')
            return;
        }).finally(() => {console.log('done'); });

        //this.#notificationContext?.peek('warning', {
        //    data: {
        //        headline: 'A thing has happened !',
        //        message: 'What that thing is? only time will tell.'
        //    }
        //});
    }

}