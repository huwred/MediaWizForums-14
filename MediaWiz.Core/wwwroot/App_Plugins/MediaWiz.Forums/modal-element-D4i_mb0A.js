import { html as p, state as f, customElement as v } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement as C } from "@umbraco-cms/backoffice/modal";
var _ = Object.defineProperty, x = Object.getOwnPropertyDescriptor, m = (t, e, o, n) => {
  for (var a = n > 1 ? void 0 : n ? x(e, o) : e, i = t.length - 1, l; i >= 0; i--)
    (l = t[i]) && (a = (n ? l(e, o, a) : l(a)) || a);
  return n && a && _(e, o, a), a;
}, y = (t, e, o) => {
  if (!e.has(t))
    throw TypeError("Cannot " + o);
}, c = (t, e, o) => {
  if (e.has(t))
    throw TypeError("Cannot add the same private member more than once");
  e instanceof WeakSet ? e.add(t) : e.set(t, o);
}, d = (t, e, o) => (y(t, e, "access private method"), o), s, h, u, b;
let r = class extends C {
  constructor() {
    super(), c(this, s), c(this, u), this.content = "";
  }
  connectedCallback() {
    super.connectedCallback();
  }
  render() {
    var t, e;
    return p`
            <umb-body-layout headline=${((t = this.data) == null ? void 0 : t.headline) ?? "Custom dialog"}>

                <uui-box>
                    <h3>${(e = this.data) == null ? void 0 : e.content}</h3>
                </uui-box>
                <uui-box>
                        <uui-button
                            id="submit"
                            color='positive'
                            look="primary"
                            label="Submit"
                            @click=${d(this, s, h)}></uui-button>
                </uui-box>
                <div slot="actions">
                        <uui-button id="cancel" label="Cancel" @click="${d(this, u, b)}">Cancel</uui-button>

            </div>
            </umb-body-layout>
        `;
  }
};
s = /* @__PURE__ */ new WeakSet();
h = function() {
  var t;
  (t = this.modalContext) == null || t.submit();
};
u = /* @__PURE__ */ new WeakSet();
b = function() {
  var t;
  (t = this.modalContext) == null || t.reject();
};
m([
  f()
], r.prototype, "content", 2);
r = m([
  v("member-custom-modal")
], r);
const M = r;
export {
  r as MemberCustomModalElement,
  M as default
};
//# sourceMappingURL=modal-element-D4i_mb0A.js.map
