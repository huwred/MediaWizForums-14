import { ManifestModal } from "@umbraco-cms/backoffice/extension-registry";

const modals: Array<ManifestModal> = [
    {
        type: 'modal',
        alias: 'member.custom.modal',
        name: 'Member custom modal',
        js: () => import('./modal-element.js')
    }
];

export const manifests = [...modals];