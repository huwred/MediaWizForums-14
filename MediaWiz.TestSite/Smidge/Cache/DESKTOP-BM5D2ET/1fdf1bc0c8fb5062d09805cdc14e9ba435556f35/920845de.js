(function(){"use strict";function sendValidationController($scope,$http,$location,editorState,navigationService){function init(){vm.loaded=!0}function close(){navigationService.hideNavigation()}function send(){var _memberId=editorState.current.id;$http.post("/SendValidation",_memberId).then(function(){close()},function(){})}var vm=this,currNode;vm.member=editorState.current.name;vm.node=null;vm.loaded=!1;vm.init=init;vm.close=close;vm.send=send;currNode=null}angular.module("umbraco").controller("MediaWizards.SendValidationController",sendValidationController)})()