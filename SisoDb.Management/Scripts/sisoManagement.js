var SisoManagement;
(function (SisoManagement) {
    var _vm,
        _supportsLocalStorage = typeof (localStorage) !== 'undefined';

    var init = function(entities){
        _vm = {
            entities: entities,
            activeTab: ko.observable(null),
            createTab: createTab,
            tabs: ko.observableArray([]),
            closeAll: function(){
                _vm.activeTab(null);
                _vm.tabs([]);
                _vm.tabQueue = [];
            },
            activateTab: function (item) {
                _vm.activeTab(item);
                if (item != null) {
                    _vm.tabQueue.push(item);
                }
            },
            tabQueue: [],
            truncate: function (str, maxLength) {
                var strValue = '' + str;
                if (strValue.length > maxLength) {
                    return strValue.substr(0, maxLength) + '..';
                } else {
                    return strValue;
                }
            }
        };

        if (_supportsLocalStorage) {

            var tabs = JSON.parse(localStorage.getItem('SisoManagement_TabStore'));
            if(tabs){
                var active = Number(localStorage.getItem('SisoManagement_Active'));
                for (var i = 0; i < tabs.length; i++) {
                    var t = tabs[i];
                    var tab = new SisoManagement.Tab(t.entity, t);
                    bindTabChangeMonitoring(tab);
                    _vm.tabs.push(tab);
                    if (tab.id === active) {
                        _vm.activeTab(tab);
                    }

                }
            }
            _vm.activeTab.subscribe(storeActive);
            _vm.tabs.subscribe(storeTabs);
            
        }

        ko.applyBindings(_vm);
    };

    var createTab = function () {
        var entity = this;
        var tab = new SisoManagement.Tab(entity); //this:entity

        bindTabChangeMonitoring(tab);
        _vm.activeTab(tab);
        _vm.tabs.push(tab);
        _vm.tabQueue.push(tab);
    };


    var createDetailsTab = function (oldTab, itemToShow) {
        var tab = new SisoManagement.Tab(oldTab.entity);
        tab.state('details');
        tab.entityId(getIdValue(oldTab.entity.IdKey, itemToShow));

        tab.loadItem();
        bindTabChangeMonitoring(tab);
        _vm.activeTab(tab);
        _vm.tabs.push(tab);
        _vm.tabQueue.push(tab);
    };

    var bindTabChangeMonitoring = function (tab) {

        if(!_supportsLocalStorage){
            return;
        }
        var bindOnChange = function (prop) {
            prop.subscribe(function (newValue) {
                storeTabs();
            });
        };

        bindOnChange(tab.state);
        bindOnChange(tab.setupCode);
        bindOnChange(tab.predicate);
        bindOnChange(tab.entityId);
        bindOnChange(tab.sortBy);
        bindOnChange(tab.sortOrder);
        bindOnChange(tab.pageSize);
    };

    var getIdValue = function (idKey, item) {
        for (var i = 0; i < item.properties.length; i++) {
            var prop = item.properties[i];
            if (prop.key === idKey) {
                return prop.value;
            }
        }
    };

    var storeTabs = function () {
        var tabs = ko.toJSON(_vm.tabs);
        console.log("storeT");
        try {
            localStorage.setItem('SisoManagement_TabStore', tabs);
        } catch (e) {
            if (e == QUOTA_EXCEEDED_ERR) {
                console.log('Quota exceeded!'); //data wasn't successfully saved due to quota exceed so throw an error
            }
        }
    };

    var storeActive = function () {
        console.log("storeActive");
        var active = (_vm.activeTab() || { id: null }).id;
        try {
            localStorage.setItem('SisoManagement_Active', active);
        } catch (e) {
            if (e == QUOTA_EXCEEDED_ERR) {
                console.log('Quota exceeded!'); //data wasn't successfully saved due to quota exceed so throw an error
            }
        }
    };

    var closeTab = function(tab){
        var needCheck = false;
        var state = tab.state();
        if (state == 'query' && tab.predicate().length > 0) {
            needCheck = true;
        }
        if (state == 'details' && tab.entityId() && tab.entityId().length > 0) {
            needCheck = true;
        }

        if (needCheck && !confirm('Are you sure you want to close this tab?')) {
            return;
        }

        var openTab = _vm.tabQueue.pop();
        if (openTab == tab) {
            var temp = _vm.tabQueue.pop();
            while (temp && temp.isDisposed) {
                temp = _vm.tabQueue.pop();
            }
            _vm.activateTab(temp);
        } else {
            _vm.tabQueue.push(openTab);
        }
        this.isDisposed = true;
        _vm.tabs.remove(tab);

        //Might need to clear tabQueue here to release memory used by the tab
    }

    SisoManagement.init = init;
    SisoManagement.createDetailsTab = createDetailsTab;
    SisoManagement.closeTab = closeTab;
})(SisoManagement || (SisoManagement = {}));