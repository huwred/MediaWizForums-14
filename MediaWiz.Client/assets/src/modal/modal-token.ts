import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

export interface MemberCustomModalData {
    headline: string;
    content: string;
}

export interface MemberCustomModalValue {
    content: string 
}

export const MEMBER_CUSTOM_MODAL = new UmbModalToken<MemberCustomModalData, MemberCustomModalValue>(
    "member.custom.modal",
    {
        modal: {
            type: 'sidebar',
            size: 'medium'
        }
    }
);