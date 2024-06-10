import { customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { MemberCustomModalData, MemberCustomModalValue } from "./modal-token";


@customElement('member-custom-modal')
export class MemberCustomModalElement extends 
    UmbModalBaseElement<MemberCustomModalData, MemberCustomModalValue> 
{
    constructor() {
        super();
    }

    connectedCallback(): void {
        super.connectedCallback();
        console.log('connectedCallback');
        //this.updateValue({content: this.data?.content});       
    }

    @state()
    content: string = '';

    #handleConfirm() {
		this.modalContext?.submit();      

	}

	#handleCancel() {
        console.log('handleCancel');
		this.modalContext?.reject();
	}

    render() {
        return html`
            <umb-body-layout headline=${this.data?.headline ?? 'Custom dialog'}>

                <uui-box>
                    <h3>${this.data?.content}</h3>
                </uui-box>
                <uui-box>
                        <uui-button
                            id="submit"
                            color='positive'
                            look="primary"
                            label="Submit"
                            @click=${this.#handleConfirm}></uui-button>
                </uui-box>
                <div slot="actions">
                        <uui-button id="cancel" label="Cancel" @click="${this.#handleCancel}">Cancel</uui-button>

            </div>
            </umb-body-layout>
        `;
    }
    
}

export default MemberCustomModalElement;