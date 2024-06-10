import { UMB_MEMBER_ENTITY_TYPE } from "@umbraco-cms/backoffice/member";
import { ManifestEntityAction } from "@umbraco-cms/backoffice/extension-registry";
import { MemberEntityAction } from "./member.entity.action";

const entityAction: ManifestEntityAction = {
    type: 'entityAction',
    kind: 'default',
    alias: 'member.entity.action',
    name: 'member action',
    weight: -100,
    forEntityTypes: [
        UMB_MEMBER_ENTITY_TYPE
    ],
    api: MemberEntityAction,
    meta: {
        icon: 'icon-message',
        label: 'Resend Validation',

    },
    conditions: [{
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Members"
    }]
}

export const manifests = [entityAction];