var m = (e, n, t) => {
  if (!n.has(e))
    throw TypeError("Cannot " + t);
};
var c = (e, n, t) => (m(e, n, "read from private field"), t ? t.call(e) : n.get(e)), r = (e, n, t) => {
  if (n.has(e))
    throw TypeError("Cannot add the same private member more than once");
  n instanceof WeakSet ? n.add(e) : n.set(e, t);
}, d = (e, n, t, o) => (m(e, n, "write to private field"), o ? o.call(e, t) : n.set(e, t), t);
import { UMB_MEMBER_ENTITY_TYPE as u } from "@umbraco-cms/backoffice/member";
import { UmbEntityActionBase as h } from "@umbraco-cms/backoffice/entity-action";
import { UmbModalToken as M, UMB_MODAL_MANAGER_CONTEXT as b } from "@umbraco-cms/backoffice/modal";
const y = new M(
  "member.custom.modal",
  {
    modal: {
      type: "sidebar",
      size: "medium"
    }
  }
);
var i;
class E extends h {
  constructor(t, o) {
    super(t, o);
    r(this, i, void 0);
    this.consumeContext(b, (s) => {
      d(this, i, s);
    });
  }
  async execute() {
    var o;
    const t = (o = c(this, i)) == null ? void 0 : o.open(this, y, {
      data: {
        headline: "Resend Validation",
        content: "Do you want to resend the validation Email?"
      }
    });
    await (t == null ? void 0 : t.onSubmit().then(() => {
      var a;
      const s = new Headers();
      s.set("Content-Type", "application/json"), s.set("Accept", "application/json");
      const l = new Request("/sendvalidation/" + ((a = this.args.unique) == null ? void 0 : a.toString()), {
        method: "GET",
        headers: s
      });
      return fetch(l).then((p) => {
        console.log("got response:", p);
      });
    }).catch(() => {
    }));
  }
}
i = new WeakMap();
const A = {
  type: "entityAction",
  kind: "default",
  alias: "member.entity.action",
  name: "member action",
  weight: -100,
  forEntityTypes: [
    u
  ],
  api: E,
  meta: {
    icon: "icon-message",
    label: "Resend Validation"
  },
  conditions: [{
    alias: "Umb.Condition.SectionAlias",
    match: "Umb.Section.Members"
  }]
}, T = [A], f = [
  {
    type: "modal",
    alias: "member.custom.modal",
    name: "Member custom modal",
    js: () => import("./modal-element-D4i_mb0A.js")
  }
], _ = [...f], g = [
  ...T,
  ..._
], x = (e, n) => {
  n.registerMany(g);
};
export {
  x as onInit
};
//# sourceMappingURL=assets.js.map
