var sisodb = {};
sisodb.management = (function () {
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
        var tab = {
            entity: this,
            isLoading: ko.observable(false), 
            state: ko.observable('dashboard'),
            setupCode: ko.observable(""),
            predicate: ko.observable(""),
            resultCount: ko.observable(null),
            entityId: ko.observable(null),
            results: ko.observableArray([]),
            entityJson: ko.observable(null),
            sortBy: ko.observable('x => x.' + this.IdKey),
            sortOrder: ko.observable('asc'),
            pageSize: ko.observable(100),
            page: ko.observable(0)
        };

        buildTabEvents(tab);

        _vm.activeTab(tab);
        _vm.tabs.push(tab);
        _vm.tabQueue.push(tab);
    }

    var buildTabEvents = function (tab) {
        tab.query = function () {
            //ErrorHandling
            tab.page(0);
            tab.loadData();
        };

        tab.loadData = function () {
            tab.isLoading(true);
            tab.results([]);
            tab.resultCount(0);
            $.ajax({
                type: 'POST',
                url: '/siso-db-management/query',
                data: {
                    entityType: this.entity.Contract,
                    setup: this.setupCode(),
                    predicate: this.predicate(),
                    orderby: this.sortBy(),
                    sortorder: this.sortOrder(),
                    pagesize: this.pageSize(),
                    page: this.page()
                },
                success: function (json) {
                    buildResultList(tab, json);
                    tab.isLoading(false);
                },
                error: tab.onUnexpectedError
            });
        };

        tab.changePage = function () {
            tab.page(this.index);

            tab.loadData();
        }

        tab.deleteByQuery = function () {
            //ErrorHandling

            if (confirm('Are you sure you want to delete all ' + tab.entity.ContractName + ' matching: ' + tab.predicate())) {
                tab.isLoading(true);
                tab.results([]);
                tab.resultCount(0);
                $.ajax({
                    type: 'POST',
                    url: '/siso-db-management/deletebyquery',
                    data: { entityType: this.entity.Contract, setup: this.setupCode(), predicate: this.predicate() },
                    success: function (number) {
                        alert(number + ' was deleted');
                        tab.isLoading(false);
                    },
                    error: tab.onUnexpectedError
                });
            }
        };

        tab.startQuery = function () {
            this.state('query');
        };

        tab.startDetails = function () {
            tab.state('details');
        };

        tab.startInsert = function () {
            tab.state('insert');
        };

        tab.regenerateIndexes = function () {
            tab.isLoading(true);
            $.ajax({
                type: 'POST',
                url: '/siso-db-management/regenerateindexes',
                data: { entityType: this.entity.Contract },
                success: function () {
                    tab.isLoading(false);
                    tab.message({
                        type: 'information', text: 'Indexes regenerated!', onClose: function () {
                            tab.message(null);
                        }
                    });
                }
            });
        };

        tab.loadItem = function () {
            //ErrorHandling
            tab.isLoading(true);
            $.ajax({
                type: 'POST',
                url: '/siso-db-management/entity',
                data: { entityType: this.entity.Contract, entityId: this.entityId() },
                success: function (json) { //json as text/html
                    tab.entityJson(json);
                    tab.isLoading(false);
                },
                error: tab.onUnexpectedError
            });
        };

        tab.deleteItem = function () {
            //ErrorHandling
            tab.isLoading(true);
            $.ajax({
                type: 'POST',
                url: '/siso-db-management/delete',
                data: { entityType: this.entity.Contract, entityId: this.entityId() },
                success: function (json) {
                    tab.isLoading(false);
                    tab.message({
                        type: 'information', text: 'Item was deleted!', onClose: function () {
                            tab.message(null);
                            tab.close();
                        }
                    });
                },
                error: tab.onUnexpectedError
            });
        };

        tab.updateItem = function () {
            //errorHandling
            tab.isLoading(true);
            $.ajax({
                type: 'POST',
                url: '/siso-db-management/update',
                data: { entityType: this.entity.Contract, entityId: this.entityId(), modifiedEntity: this.entityJson() },
                success: function (json) { //json as text/html
                    tab.isLoading(false);
                    tab.message({
                        type: 'information', text: 'Item updated!', onClose: function () {
                            tab.message(null);
                        }
                    });
                },
                error: tab.onUnexpectedError
            });
        };

        tab.insertItem = function () {
            //ErrorHandling
            tab.isLoading(true);
            $.ajax({
                type: 'POST',
                url: '/siso-db-management/insert',
                data: { entityType: this.entity.Contract, json: this.entityJson() },
                success: function (json) { //json as text/html
                    tab.isLoading(false);
                    tab.message({
                        type: 'information', text: 'Items inserted!', onClose: function () {
                            tab.message(null);
                        }
                    });
                },
                error: tab.onUnexpectedError
            });
        }

        tab.tabText = ko.computed(function () {
            switch (this.state()) {
                case 'dashboard':
                    return "Dashboard";
                case 'query':
                    return "Q: " + this.predicate();
                case 'details':
                    return "D: " + this.entityId();
                case 'insert':
                    return "Insert";
                default:
                    return 'unknown';
            }
        }, tab);

        tab.showingText = ko.computed(function () {
            var min = this.pageSize() * this.page();
            var max = this.results().length + min;

            return (min + 1) + "-" + max;
        }, tab);

        tab.pageLinks = ko.computed(function () {
            var page = this.page();
            var pages = Math.ceil(this.resultCount() / this.pageSize());

            var links = [];
            var start = Math.max(0, page - 3);
            var max = Math.min(pages - 1, page + 3);
            if (start > 0) {
                links.push({ index: 0, text: "First", cssClass: 'endNav' });
            }
            for (var i = start; i <= max; i++) {
                links.push({ index: i, text: i, cssClass: i == page ? 'active' : '' });
            }
            if (max < pages - 1) {
                links.push({ index: pages - 1, text: "Last", cssClass: 'endNav' });
            }

            return links;
        }, tab);

        tab.close = function () {
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
            if (openTab == this) {
                var temp = _vm.tabQueue.pop();
                while (temp && temp.isDisposed) {
                    temp = _vm.tabQueue.pop();
                }
                _vm.activateTab(temp);
            } else {
                _vm.tabQueue.push(openTab);
            }
            this.isDisposed = true;
            _vm.tabs.remove(this);

            //Might need to clear tabQueue here to release memory used by the tab
        };

        tab.onUnexpectedError = function (xhr, textStatus, errorThrown) {
            tab.isLoading(false);
            tab.message({
                type: 'error', text: errorThrown + ":" + xhr.responseText, onClose: function () {
                    tab.message(null);
                }
            });
        };

        tab.message = ko.observable(null);

        tab.showMessage = ko.computed(function () {
            return tab.message() != null;
        }, tab);

        tab.messageClass = ko.computed(function () {
            return tab.message()
            ? "message " + tab.message().type
            : "nomessage";
        }, tab);

        tab.properties = [];
        for (var i = 0; i < tab.entity.Properties.length; i++) {
            var property = tab.entity.Properties[i];
            var parts = property.split('.');

            tab.properties.push({ name: property, parts: parts })

        }
    }

    var buildResultList = function (tab, json) {
        tab.resultCount(json.TotalMatches);
        

        var results = [];
        for (var i = 0; i < json.Entities.length; i++) {
            var obj = json.Entities[i];
            var resultObject = {
                object: obj,
                properties: [],
                click: function (event) {
                    createDetailsTab(tab, this);
                }
            };
            for (var j = 0; j < tab.properties.length; j++) {
                var prop = tab.properties[j];

                resultObject.properties.push(getValue(prop, obj));
            }
            results.push(resultObject);
        }
        tab.results(results);
    }

    var getValue = function (property, obj) {
        var temp = obj[property.parts[0]];
        for (var k = 1; k < property.parts.length; k++) {
            if (typeof temp == 'undefined') {
                return { value: '', key: property.name };
            }
            temp = temp[property.parts[k]];
        }
        return { value: typeof temp == 'undefined' ? '' : temp, key: property.name };
    };

    var createDetailsTab = function (oldTab, itemToShow) {
        var tab = {
            entity: oldTab.entity,
            isLoading: ko.observable(false),
            state: ko.observable('details'),
            setupCode: ko.observable(null),
            predicate: ko.observable(null),
            entityId : ko.observable(getIdValue(oldTab.entity.IdKey, itemToShow)),
            resultCount: ko.observable(0),
            results: ko.observableArray([]),
            entityJson: ko.observable(null),
            sortBy: ko.observable('x => x.' + oldTab.IdKey),
            sortOrder: ko.observable(oldTab.sortOrder()),
            pageSize: ko.observable(oldTab.pageSize()),
            page: ko.observable(0)
        };

        buildTabEvents(tab);
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

    var isValidJson = function(str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }

    return {
        init : init
    };
})();