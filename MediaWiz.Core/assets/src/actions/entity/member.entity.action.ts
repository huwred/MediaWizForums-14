import { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbEntityActionArgs, UmbEntityActionBase } from "@umbraco-cms/backoffice/entity-action";
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalManagerContext } from "@umbraco-cms/backoffice/modal";
import { MEMBER_CUSTOM_MODAL } from "../../modal/modal-token.ts";
import { UmbMemberDetailRepository  } from '@umbraco-cms/backoffice/member';
export class MemberEntityAction extends UmbEntityActionBase<UmbMemberDetailRepository> {
    #modalManagerContext?: UmbModalManagerContext;
    constructor(host: UmbControllerHostElement, args: UmbEntityActionArgs<UmbMemberDetailRepository>)
    {
        super(host, args)
        // Fetch/consume the contexts & assign to the private fields
        this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
            this.#modalManagerContext = instance;
        });

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

            const request: RequestInfo = new Request('/sendvalidation/' + this.args.unique?.toString(), {
                method: 'GET',
                headers: headers,
            })
            // Send the request and print the response
            return fetch(request)
            .then(res => {
                console.log("got response:", res)
            })
        }).catch(() => {
            return;
        });

    }

}