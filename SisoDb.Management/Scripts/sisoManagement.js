var SisoManagement;
(function (SisoManagement) {
    var _vm;
    var init = function(entities){
        _vm = {
            entities: entities,
            activeTab: ko.observable(null),
            createTab: createTab,
            tabs: ko.observableArray([]),
            activateTab: function (item) {
                _vm.activeTab(item);
                if (item != null) {
                    _vm.tabQueue.push(item);
                }
            },
            tabQueue: [],
            truncate: function (str, maxLength) {
                var strValue = "" + str;
                if (strValue.length > maxLength) {
                    return strValue.substr(0, maxLength) + "..";
                } else {
                    return strValue;
                }
            }
        };

        ko.applyBindings(_vm);
    };

    var createTab = function () {
        var entity = this;
        var tab = new SisoManagement.Tab(entity); //this:entity

        _vm.activeTab(tab);
        _vm.tabs.push(tab);
        _vm.tabQueue.push(tab);
    }


    var createDetailsTab = function (oldTab, itemToShow) {
        var tab = new SisoManagement.Tab(oldTab.entity);
        tab.state('details');
        tab.entityId(getIdValue(oldTab.entity.IdKey, itemToShow));

        tab.loadItem();

        _vm.activeTab(tab);
        _vm.tabs.push(tab);
        _vm.tabQueue.push(tab);
    };

    var getIdValue = function (idKey, item) {
        for (var i = 0; i < item.properties.length; i++) {
            var prop = item.properties[i];
            if (prop.key === idKey) {
                return prop.value;
            }
        }
    }

    SisoManagement.init = init;
    SisoManagement.createDetailsTab = createDetailsTab;
})(SisoManagement || (SisoManagement = {}));