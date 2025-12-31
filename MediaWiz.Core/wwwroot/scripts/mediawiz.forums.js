// Toggle sidebar collapse
document.getElementById('toggleWizBtn').addEventListener('click', function () {
    document.getElementById('sidebar').classList.toggle('collapsed');
});

var MediaWiz = MediaWiz ||
{
    tools: "code undo redo | styleselect | bullist numlist | indent outdent | link codesample",
    returnUrl: "",
    currLang: getLang(),
    editPost: function(postId) {
        $.ajax({
            type: "GET",
            url: "/umbraco/surface/forumssurface/editPost/" + postId,
            success: function (result) {
                $("#partial-form").html(result);

            },
            error: function (error) {
                createAlert('', 'Failed!', error, 'danger', true, true, 'pageErrors');
            }
        });
    },
    deletePost: function (postId) {
        (async () => {
            const result = await b_confirm("Are you sure you want to delete this post?")
            if (result) {
                var del = $.get("/deletepost/" + postId);
                del.done(function (data, status) {
                    location.reload();
                });
                del.fail(function (result) {
                    createAlert('', 'Failed!', 'Sorry, there was an error deleting this post', 'danger', true, true, 'pageErrors');
                });
            }
        })();
    },
    markAnswer: function (postId) {
        (async () => {
            const result = await b_confirm("Are you sure you want to mark this post as the answer?")
            if (result) {
                var answered = $.get("/markanswer/" + postId);
                answered.done(function (data, status) {
                    location.reload();
                });
                answered.fail(function (result) {
                    createAlert('', 'Failed!', 'Sorry, there was an error marking this post as the answer', 'danger', true, true, 'pageErrors');
                });
            }
        })();

    },
    lockPost: function (postId) {
        (async () => {
            const result = await b_confirm("Are you sure you want to lock/unlock this post?")
            if (result) {
                var locking = $.get("/lockpost/" + postId);
                locking.done(function (data, status) {
                    location.reload();
                });
                locking.fail(function (result) {
                    createAlert('', 'Failed!', 'Sorry, there was an error locking this post', 'danger', true, true, 'pageErrors');
                });
            }
        })();
    },
    approvePost: function (postId) {
        (async () => {
            const result = await b_confirm("Are you sure you want to Approve this post?")
            if (result) {
                var approved = $.get("/approve/" + postId);
                approved.done(function (data, status) {
                    location.reload();
                });
                approved.fail(function (result) {
                    createAlert('', 'Failed!', 'Sorry, there was an error approving the post', 'danger', true, true, 'pageErrors');
                });
            }
        })();
    },
    lockUser: function (user, mode) {
        (async () => {
            const result = await b_confirm("Are you sure you want to lock this Member?")
            if (result) {
                $.ajax({
                    type: "GET",
                    url: "/lockuser/" + user + "/?mode=" + mode,
                    success: function (result) {
                        if (result) {
                            location.reload(true);
                        }
                    },
                    error: function (error) {
                        createAlert('', 'Failed!', error, 'danger', true, true, 'pageErrors');
                    }
                });
            }
        })();

    },
    captchaCheck: function(answer, callback) {
        $.ajax({
            url: '/captchacheck/' + answer,
            type: 'GET',
            success: function(data) {

                if (callback) {callback(data); }

            },
            error: function(jqXHR, exception) {
                createAlert('', 'Failed!', exception, 'danger', true, true, 'pageErrors');
                return false;
            }
        });
    },
    InitTinyMce:  function (selector) {
        window.tinymce.init({
            selector: selector,
            browser_spellcheck: true,
            contextmenu: false,
            plugins: "link lists anchor codesample image code emoticons",
            toolbar: MediaWiz.tools,
            file_picker_types: "image",
            images_upload_url: "/forumupload",
            images_reuse_filename: true,
            statusbar: false,
            menubar: false,
            relative_urls : false,
            remove_script_host : true,
            document_base_url : window.location.protocol + "//" + window.location.host + "/",
            convert_urls: true,
            content_css: false,
            content_style: `
                blockquote {
                    border-left: 4px solid #ddd;
                    padding: 0 15px;
                    color: #777;
                }
                .mce-content-body img {
                    max-width: 99% !important;
                    height: auto;
                } `
        });
        document.addEventListener("focusin",
            (e) => {
                if (e.target.closest(".tox-tinymce-aux, .moxman-window, .tam-assetmanager-root") !== null) {
                    e.stopImmediatePropagation();
                }
            });
    }
}

$(document).ready(function() {

    if (MediaWiz.returnUrl.length > 1) {
        window.pageRedirect(MediaWiz.returnUrl);
    }

    $(".btn-cancel").on("click", function(e) {
        history.back();
    });
    $(".post-quote").on("click", function(e) {
        e.stopPropagation();
        e.preventDefault();

        tinymce.activeEditor.setContent("<blockquote>" + $("#postcontent_" + $(this).data("postid")).html() + "</blockquote><br/> ");
        goToTheEnd();
    });
    $(".post-delete").on("click", function(e) {
        e.stopPropagation();
        e.preventDefault();
        MediaWiz.deletePost($(this).data("postid"));
    });

    $(".post-lock").on("click",function (e) {
        e.stopPropagation();
        e.preventDefault();
        MediaWiz.lockPost($(this).data("postid"));
    });
    $(".post-approve").on("click",function (e) {
        e.stopPropagation();
        e.preventDefault();
        MediaWiz.approvePost($(this).data("postid"));
    });
    $(".post-answer").on("click",function (e) {

        e.stopPropagation();
        e.preventDefault();
        MediaWiz.markAnswer($(this).data("postid"));
    });
    $(".post-edit").on("click",function (e) {
        e.stopPropagation();
        e.preventDefault();
        MediaWiz.editPost($(this).data("postid"));

    });

    $(".lock-user").on("click",function(e) {
        e.stopPropagation();
        e.preventDefault();
        MediaWiz.lockUser($(this).data("userid"), $(this).data("mode"));
    });

    $("#editPostModal").on("show.bs.modal",function() {
        setTimeout(function() {
            MediaWiz.InitTinyMce("#partial-form textarea");
        }, 300);
    });

    $("#editPostModal").on("hide.bs.modal", function () {
        tinymce.remove("#partial-form textarea");
    });
});
function getLang() {
    if (navigator.languages != undefined) 
        return navigator.languages[0]; 
    return navigator.language;
}
function goToTheEnd() {
    var ed=tinyMCE.activeEditor;
    var root=ed.dom.getRoot();  // This gets the root node of the editor window
    var lastnode=root.childNodes[root.childNodes.length-1]; // And this gets the last node inside of it, so the last <p>...</p> tag
    if (tinymce.isGecko) {
        // But firefox places the selection outside of that tag, so we need to go one level deeper:
        lastnode=lastnode.childNodes[lastnode.childNodes.length-1];
    }
    // Now, we select the node
    ed.selection.select(lastnode);
    // And collapse the selection to the end to put the caret there:
    ed.selection.collapse(false);
}

$( "li.reply" ).hover(
    function() {
        $(this).find(".tool-label").show();
    }, function() {
        $(this).find(".tool-label").hide();
    }
);
$( "li.topic" ).hover(
    function() {
        $(this).find(".tool-label").show();
    }, function() {
        $(this).find(".tool-label").hide();
    }
);

function createAlert(title, summary, details, severity, dismissible, autoDismiss, appendToId) {
    var iconMap = {
        info: "bi bi-info-circle-fill",
        success: "bi bi-hand-thumbs-up-fill",
        warning: "bi bi-exclamation-triangle-fill",
        danger: "bi bi-exclamation-circle-fill"
    };

    var iconAdded = false;

    var alertClasses = ["alert", "animated", "flipInX"];
    alertClasses.push("alert-" + severity.toLowerCase());

    if (dismissible) {
        alertClasses.push("alert-dismissible");
    }

    var msgIcon = $("<i />", {
        "class": iconMap[severity] // you need to quote "class" since it's a reserved keyword
    });

    var msg = $("<div />", {
        "class": alertClasses.join(" ") // you need to quote "class" since it's a reserved keyword
    });

    if (title) {
        var msgTitle = $("<h4 />", {
            html: title
        }).appendTo(msg);

        if (!iconAdded) {
            msgTitle.prepend(msgIcon);
            iconAdded = true;
        }
    }

    if (summary) {
        var msgSummary = $("<strong />", {
            html: summary
        }).appendTo(msg);

        if (!iconAdded) {
            msgSummary.prepend(msgIcon);
            iconAdded = true;
        }
    }

    if (details) {
        var msgDetails = $("<p />", {
            html: details
        }).appendTo(msg);

        if (!iconAdded) {
            msgDetails.prepend(msgIcon);
            iconAdded = true;
        }
    }


    if (dismissible) {
        var msgClose = $("<span />", {
            "class": "close", // you need to quote "class" since it's a reserved keyword
            "data-dismiss": "alert",
            html: "<i class='bi bi-x-circle'></i>"
        }).appendTo(msg);
    }

    $('#' + appendToId).prepend(msg);

    if (autoDismiss) {
        setTimeout(function () {
            msg.addClass("flipOutX");
            setTimeout(function () {
                msg.remove();
            }, 1000);
        }, 5000);
    }
}

async function b_confirm(msg) {
    const modalElem = document.createElement('div');
    modalElem.id = "modal-confirm";
    modalElem.className = "modal";
    modalElem.innerHTML = `
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content bg-confirm text-bg-confirm">
                <div class="modal-body fs-6">
                    <p>${msg}</p>
                </div>
                <div class="modal-footer" style="border-top:0px">
                    <button id="modal-btn-accept" type="button" class="btn btn-danger">Yes</button>
                    <button id="modal-btn-cancel" type="button" class="btn btn-success">Cancel</button>
                </div>
            </div>
        </div>`;
    const myModal = new bootstrap.Modal(modalElem, {
        keyboard: false,
        //backdrop: 'static'
    });
    myModal.show();

    return new Promise((resolve, reject) => {
        document.body.addEventListener('click', response)

        function response(e) {
            let bool = false
            if (e.target.id == 'modal-btn-cancel') bool = false
            else if (e.target.id == 'modal-btn-accept') bool = true
            else return

            document.body.removeEventListener('click', response)
            myModal.hide()
            modalElem.remove()
            resolve(bool)
        }
    })
}