var SisoManagement;
(function (SisoManagement) {
    var Tab = (function () {
        function Tab(entity, values) {
            /*values should keep the same interface as Tab but not have 
            observable properties. This is primarily to support storing tabs */
            values = values || {};
            var self = this;
            this.id = Number(values.id || +new Date); //Use a timestamp as id. It will only be used to identify the tab later on
            this.entity = entity;
            this.isLoading = ko.observable(false); 
            this.state = ko.observable(values.state || 'dashboard');
            this.setupCode = ko.observable(values.setupCode || '');
            this.predicate = ko.observable(values.predicate || '');
            this.resultCount = ko.observable(null);
            this.entityId = ko.observable(values.entityId || null);
            this.results = ko.observableArray([]);
            this.entityJson = ko.observable(null);
            this.sortBy = ko.observable( values.sortBy || ('x => x.' + entity.IdKey));
            this.sortOrder = ko.observable(values.sortOrder || 'asc');
            this.pageSize = ko.observable(Number(values.pageSize || 100));
            this.page = ko.observable(0);
            this.message = ko.observable(null);
            this.properties = [];


            this.tabText = ko.computed(function () {
                switch (this.state()) {
                    case 'dashboard':
                        return 'Dashboard';
                    case 'query':
                        return 'Q: ' + this.predicate();
                    case 'details':
                        return 'D: ' + this.entityId();
                    case 'insert':
                        return 'Insert';
                    default:
                        return 'unknown';
                }
            }, this);

            this.showingText = ko.computed(function () {
                var min = this.pageSize() * this.page();
                var max = this.results().length + min;

                return (min + 1) + '-' + max;
            }, this);

            this.pageLinks = ko.computed(function () {
                var page = this.page();
                var pages = Math.ceil(this.resultCount() / this.pageSize());

                var links = [];
                var start = Math.max(0, page - 3);
                var max = Math.min(pages - 1, page + 3);
                if (start > 0) {
                    links.push({ index: 0, text: 'First', cssClass: 'endNav' });
                }
                for (var i = start; i <= max; i++) {
                    links.push({ index: i, text: i, cssClass: i == page ? 'active' : '' });
                }
                if (max < pages - 1) {
                    links.push({ index: pages - 1, text: 'Last', cssClass: 'endNav' });
                }

                return links;
            }, this);

            this.messageClass = ko.computed(function () {
                return this.message()
                ? 'message ' + this.message().type
                : 'nomessage';
            }, this);

            this.showMessage = ko.computed(function () {
                return this.message() != null;
            }, this);

            for (var i = 0; i < this.entity.Properties.length; i++) {
                var property = this.entity.Properties[i];
                var parts = property.split('.');

                this.properties.push({ name: property, parts: parts })

            }

            this.changePage = function (item) {
                self.page(item.index);

                self.loadData();
            };
        }


        Tab.prototype.startQuery = function () {
            this.state('query');
        };

        Tab.prototype.startDetails = function () {
            this.state('details');
        };

        Tab.prototype.startInsert = function () {
            this.state('insert');
        };

        var makeAjaxCall = function(tab, options){
            options.context = tab;
            $.ajax(options);
        }

        Tab.prototype.regenerateIndexes = function () {
            var tab = this;
            tab.isLoading(true);
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'regenerateindexes',
                data: { entityType: this.entity.Contract },
                success: function () {
                    tab.isLoading(false);
                    tab.message({
                        type: 'information', text: 'Indexes regenerated!', onClose: function () {
                            tab.message(null);
                        }
                    });
                },
                error: tab.onUnexpectedError
            });
        };

        Tab.prototype.insertschema = function () {
            var tab = this;
            tab.isLoading(true);
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'insertschema',
                data: { entityType: this.entity.Contract },
                success: function () {
                    tab.isLoading(false);
                    tab.message({
                        type: 'information', text: 'Schema inserted!', onClose: function () {
                            tab.message(null);
                        }
                    });
                },
                error: tab.onUnexpectedError
            });
        };

        Tab.prototype.query = function () {
            //ErrorHandling
            this.page(0);
            this.loadData();
        };

        Tab.prototype.loadData = function () {
            this.isLoading(true);
            this.results([]);
            this.resultCount(0);
            var tab = this;
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'query',
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
                error: this.onUnexpectedError
            });
        };

        Tab.prototype.deleteByQuery = function () {
            //ErrorHandling

            if (confirm('Are you sure you want to delete all ' + this.entity.ContractName + ' matching: ' + this.predicate())) {
                this.isLoading(true);
                this.results([]);
                this.resultCount(0);
                var tab = this;
                makeAjaxCall(tab, {
                    type: 'POST',
                    url: 'deletebyquery',
                    data: { entityType: this.entity.Contract, setup: this.setupCode(), predicate: this.predicate() },
                    success: function (number) {
                        tab.isLoading(false);
                        tab.message({
                            type: 'information', text: number + ' ' + tab.entity.ContractName + ' was deleted', onClose: function () {
                                tab.message(null);
                            }
                        });
                    },
                    error: this.onUnexpectedError
                });
            }
        };

        Tab.prototype.loadItem = function () {
            //ErrorHandling
            this.isLoading(true);
            var tab = this;
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'entity',
                data: { entityType: this.entity.Contract, entityId: this.entityId() },
                success: function (json) { //json as text/html
                    var formatedJson = JSON.stringify(JSON.parse(json), null, 4);
                    tab.entityJson(formatedJson);
                    tab.isLoading(false);
                },
                error: tab.onUnexpectedError
            });
        };

        Tab.prototype.deleteItem = function () {
            //ErrorHandling
            var tab = this;
            tab.isLoading(true);
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'delete',
                data: { entityType: tab.entity.Contract, entityId: tab.entityId() },
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

        Tab.prototype.updateItem = function () {
            //errorHandling
            var tab = this;
            tab.isLoading(true);
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'update',
                data: { entityType: this.entity.Contract, entityId: tab.entityId(), modifiedEntity: tab.entityJson() },
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

        Tab.prototype.insertItem = function () {
            //ErrorHandling
            var tab = this;
            tab.isLoading(true);
            makeAjaxCall(tab, {
                type: 'POST',
                url: 'insert',
                data: { entityType: tab.entity.Contract, json: tab.entityJson() },
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
        };

        Tab.prototype.close = function () {
            SisoManagement.closeTab(this);
        };

        Tab.prototype.onUnexpectedError = function (xhr, textStatus, errorThrown) {
            var tab = this;
            tab.isLoading(false);
            tab.message({
                type: 'error', text: errorThrown + ':' + xhr.responseText, onClose: function () {
                    tab.message(null);
                }
            });
        };

        return Tab;
    })();
    SisoManagement.Tab = Tab;

    var buildResultList = function (tab, json) {
        tab.resultCount(json.TotalMatches);

        var results = [];
        for (var i = 0; i < json.Entities.length; i++) {
            var obj = json.Entities[i];
            var resultObject = {
                object: obj,
                properties: [],
                click: function (event) {
                    SisoManagement.createDetailsTab(tab, this);
                }
            };
            for (var j = 0; j < tab.properties.length; j++) {
                var prop = tab.properties[j];

                resultObject.properties.push(getValue(prop, obj));
            }
            results.push(resultObject);
        }
        tab.results(results);
    };

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

    var isValidJson = function (str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    };

})(SisoManagement || (SisoManagement = {}));