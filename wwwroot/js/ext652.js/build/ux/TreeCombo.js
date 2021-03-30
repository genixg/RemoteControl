
 //Pronin E - 16.07.2015 добавлена проверка на вызов рекурсивной ф-ии checkNodes(...),
 //необоснованное её использованию приводит к сильным тормозам при достаточно большом кол-ве записей в store

/*
Tree combo
Use with 'Ext.data.TreeStore'
 
If store root note has 'checked' property tree combo becomes multiselect combo (tree store must have records with 'checked' property)
 
Has event 'itemclick' that can be used to capture click
 
Options:
selectChildren - if set true and if store isn't multiselect, clicking on an non-leaf node selects all it's children
canSelectFolders - if set true and store isn't multiselect clicking on a folder selects that folder also as a value
 
Use:
 
single leaf node selector:
selectChildren: false
canSelectFolders: false
- this will select only leaf nodes and will not allow selecting non-leaf nodes
 
single node selector (can select leaf and non-leaf nodes)
selectChildren: false
canSelectFolders: true
- this will select single value either leaf or non-leaf
 
children selector:
selectChildren: true
canSelectFolders: true
- clicking on a node will select it's children and node, clicking on a leaf node will select only that node
 
This config:
selectChildren: true
canSelectFolders: false
- is invalid, you cannot select children without node
 
Thanks to http://extjs.dariofilkovic.com/
*/
//if (!Array.prototype.indexOf) {
//    Array.prototype.indexOf = function (obj, start) {
//        for (var i = (start || 0), j = this.length; i < j; i++) {
//            if (this[i] === obj) {
//                return i;
//            }
//        }
//        return -1;
//    };
//}

Ext.define('Ext.ux.SearchRemoteField', {
    extend: 'Ext.form.field.Trigger',

    alias: 'widget.searchremotefield',

    trigger1Cls: Ext.baseCSSPrefix + 'form-clear-trigger',

    trigger2Cls: Ext.baseCSSPrefix + 'form-search-trigger',

    hasSearch: false,
    highlightFoundNodes: false,
    filterFirstValue: false,
    filterInterval: 0,
    filterRemote: true,
    filterFn: null,

    store: null,

    initComponent: function () {
        var me = this;

        //с версии 5.0.0 не обязательно, см. https://github.com/ivan-novakov/extjs-upload-widget/issues/26
        //        me.addEvents({
        //            'beforeSearchClick': true
        //        });

        me.callParent(arguments);
        me.on('specialkey', function (f, e) {
            if (e.getKey() == e.ENTER) {
                me.onTrigger2Click();
            }
        });
        if (me.filterInterval > 0) {
            me.on('change', function (sender, newValue, oldValue, eOpts) {
                if (sender.filterTimeoutId) {
                    clearTimeout(sender.filterTimeoutId);
                    delete sender.filterTimeoutId;
                }
                if (me.filterFirstValue) {
                    sender.filterTimeoutId = setTimeout(function () { me.onTrigger2Click(); }, me.filterInterval);
                }
                me.filterFirstValue = true;
            });
        }
    },

    afterRender: function () {
        this.callParent();
        this.triggerCell.item(0).setDisplayed(false);
    },

    clearFilter: function () {
        var me = this;
        me.onTrigger1Click();
    },

    onTrigger1Click: function () {
        var me = this;

        if (me.hasSearch) {
            me.setValue('');
            me.hasSearch = false;
            if (me.filterRemote) {
                me.store.clearFilterRemote();
            }
            else {
                me.store.clearFilter();
            }
            
            me.triggerCell.item(0).setDisplayed(false);
            //me.updateLayout();
        }
    },

    onTrigger2Click: function () {
        var me = this,
            value = me.getValue();

        if (me.fireEvent('beforeSearchClick', me) !== false && value.length > 0) {
            // Param name is ignored here since we use custom encoding in the proxy.
            // id is used by the Store to replace any previous filter
            me.hasSearch = true;
            if (me.filterRemote) {
                me.store.filterParams.params.filterValue = value;
                me.store.filterRemote();
            }
            else if (me.filterFn && typeof (me.filterFn) === "function") {
                me.store.filterBy(me.filterFn, me);
            }
            me.triggerCell.item(0).setDisplayed(true);
            me.filterFirstValue = true;
            //me.updateLayout();
        }
    },

    highlightNodes: function (nodes) {
        var me = this,
            len = nodes.length,
            matchCls = 'x-livesearch-match',
            caseSensitive = false,
            tagsRe = /<[^>]*>/gm,
        // DEL ASCII code
            tagsProtect = '\x0f',
        // detects regexp reserved word
            regExpProtect = /\\|\/|\+|\\|\.|\[|\]|\{|\}|\?|\$|\*|\^|\|/gm,
            value = me.getValue(),
            searchRegExp = new RegExp(value, 'g' + (caseSensitive ? '' : 'i')),
            td, cell, matches, cellHTML, el;
        if (me.hasSearch && value && me.highlightFoundNodes) {
            for (var i = 0; i < len; i++) {
                el = Ext.fly(nodes[i]);
                if (el) {
                    td = el.down('.x-grid-cell-treecolumn');
                    //while (td) {
                    cell = td.down('.x-grid-cell-inner');
                    matches = cell.dom.innerHTML.match(tagsRe);
                    cellHTML = cell.dom.innerHTML.replace(tagsRe, tagsProtect);

                    // populate indexes array, set currentIndex, and replace wrap matched string in a span
                    cellHTML = cellHTML.replace(searchRegExp, function (m) {
                        return '<span class="' + matchCls + '">' + m + '</span>';
                    });
                    // restore protected tags
                    Ext.each(matches, function (match) {
                        cellHTML = cellHTML.replace(tagsProtect, match);
                    });
                    // update cell html
                    cell.dom.innerHTML = cellHTML;
                    //td = td.next();
                    //}
                }
            }
        }
    }
});

Ext.define('Ext.ux.TreeCombo', {
    extend: 'Ext.form.field.Picker',
    alias: 'widget.treecombo',
    tree: false,

    records: [],
    recursiveRecords: [],
    ids: [],
    selectChildren: true,
    canSelectFolders: true,
    multiselect: false,
    displayField: 'text',
    valueField: 'id',
    treeWidth: 300,
    matchFieldWidth: false,
    treeHeight: 400,
    afterLoadSetValue: false,

    showFilterField: false,
    expandAllAfterFilter: false,

    constructor: function (config) {
        //с версии 5.0.0 не обязательно, см. https://github.com/ivan-novakov/extjs-upload-widget/issues/26
//        this.addEvents({
//            "itemclick": true,
//            'beforeitemclick': true
//        });

        this.listeners = config.listeners;
        this.treeConfig = config.treeConfig;
        this.callParent(arguments);
    },
    initComponent: function () {
        var me = this;

        me.filterField = Ext.create('Ext.ux.SearchRemoteField', {
            store: me.store,
            hidden: !me.showFilterField,
            highlightFoundNodes: me.highlightFoundNodes,
            emptyText: me.emptyFilterText,
            filterInterval: me.filterInterval,
            filterFirstValue: me.filterFirstValue,
            width: me.treeWidth
        });

        me.tree = Ext.create('Ext.tree.Panel', Ext.apply({
            alias: 'widget.assetstree',
            hidden: true,
            minHeight: 300,
            rootVisible: (typeof me.rootVisible != 'undefined') ? me.rootVisible : true,
            floating: true,
            lines: true,
            useArrows: true,
            width: me.treeWidth,
            autoScroll: true,
            height: me.treeHeight,
            store: me.store,
            listeners: {
                load: function (store, records) {
                    if (me.afterLoadSetValue != false) {
                        me.setValue(me.afterLoadSetValue);
                    }
                },
                checkchange: function (node, checked, opt) {                    //IE hack for itemclick event                  
                    node.set("checked", !checked);
                    me.itemTreeClick(me.tree.getView(), node, node, 1, null, null, me);
                },
                itemclick: function (view, record, item, index, e, eOpts) {
                    me.itemTreeClick(view, record, item, index, e, eOpts, me);
                }
            },
            //tbar: (me.showFilterField ? [me.filterField]: undefined)
            dockedItems: (me.showFilterField ? [{
                xtype: 'toolbar',
                dock: 'top',
                layout: 'fit',
                items: [me.filterField]
            }] : undefined)
        }, me.treeConfig));

        //проверяем наличие плагина удаленной фильтрации
        var viewPlugins = me.tree.getView().plugins;
        var remoteFilterPlugin = null;
        var plugin = null;
        if (viewPlugins) {
            for (var i = 0; i < viewPlugins.length; i++) {
                plugin = viewPlugins[i];
                if (plugin.ptype == 'treestoreremotefilter') {
                    remoteFilterPlugin = plugin;
                    break;
                }
            }
        }
        //если такого плагина нет - блокируем панель поиска
        if (!remoteFilterPlugin) {
            me.filterField.hide();
            var tbar = me.tree.getDockedItems('toolbar[dock="top"]');
            if (tbar && tbar.length > 0) {
                me.tree.dockedItems.remove(tbar[0]);
            }
        }

        me.storeItemExpand = function (node, index, item, eOpts) {
            var view = this,
                nodes = [],
            //children = node.childNodes,
            //len = children.length,
                filterField = me.filterField;

            var getNodesFn = function (view, nodes, record) {
                var children = record.childNodes,
                    len = children.length,
                    child = null,
                    el = null;
                for (var i = 0; i < len; i++) {
                    child = children[i];
                    el = view.getNode(child);
                    if (el) {
                        nodes.push(el);
                    }
                    if (child.isExpanded()) {
                        getNodesFn(view, nodes, child);
                    }
                }
            }

            if (filterField.hasSearch && filterField.highlightFoundNodes) {
                //                for (var i = 0; i < len; i++) {
                //                    nodes.push(view.getNode(children[i]));
                //                }
                getNodesFn(view, nodes, node);
                filterField.highlightNodes(nodes);
            }
        };

        me.storeItemUpdate = function (record, index, node, eOpts) {
            var filterField = me.filterField;
            if (filterField.hasSearch && filterField.highlightFoundNodes) {
                filterField.highlightNodes([this.getNode(record)]);
            }
        };

        me.store.on('filterRemote', function (store, records) {
            var view = me.tree.getView(),
                nodes = view.getNodes();
            //if (records && records.length > 0) {
                view.un('afteritemexpand', me.storeItemExpand);
                view.un('itemupdate', me.storeItemUpdate);
                view.on('afteritemexpand', me.storeItemExpand);
                view.on('itemupdate', me.storeItemUpdate);
//            }
//            else {
                me.filterField.highlightNodes(nodes);
            //}
        }, me.store);

        if (me.expandAllAfterFilter) {
            me.store.on('filterRemoteOnly', function (store, records) {
                var view = me.tree.getView(),
                nodes = view.getNodes();

                me.tree.expandAll();
            }, me.store);
        }

        if (me.tree.getRootNode().get('checked') != null)
            me.multiselect = true;

        this.createPicker = function () {
            var me = this;
            return me.tree;
        };

        if (!me.displayTpl) {
            me.displayTpl = new Ext.XTemplate(
                '<tpl for=".">' +
                    '{[Ext.util.Format.htmlDecode((typeof values === "string" ? values : values["' + me.displayField + '"]))]}' +
                    '<tpl if="xindex < xcount">, </tpl>' +
                '</tpl>'
            );
        } else if (Ext.isString(me.displayTpl)) {
            me.displayTpl = new Ext.XTemplate(me.displayTpl);
        }

        this.callParent(arguments);
    },
    destroy: function () {
        this.tree.destroy();
        this.callParent(arguments);
    },
    itemTreeClick: function (view, record, item, index, e, eOpts, treeCombo) {
        var me = treeCombo,
                checked = !record.get('checked'); //it is still not checked if will be checked in this event

        var res = me.fireEvent('beforeitemclick', me, record, item, index, e, eOpts);
        if (!res) {
            return false;
        }

        if (me.multiselect == true)
            record.set('checked', checked); //check record

        var node = me.tree.getRootNode().findChild(me.valueField, record.get(me.valueField), true);
        if (node == null) {
            if (me.tree.getRootNode().get(me.valueField) == record.get(me.valueField))
                node = me.tree.getRootNode();
            else
                return false;
        }

        if (me.multiselect == false)
            me.ids = [];

        //if it can't select folders and it is a folder check existing values and return false
        if (me.canSelectFolders == false && record.get('leaf') == false) {
            me.setRecordsValue(view, record, item, index, e, eOpts, treeCombo);
            return false;
        }

        //if record is leaf
        if (record.get('leaf') == true) {
            if (checked == true) {
                me.addIds(record);
            } else {
                me.removeIds(record);
            }
        } else {
            //it's a directory
            me.recursiveRecords = [];
            if (checked == true) {
                if (me.multiselect == false) {
                    if (me.canSelectFolders == true)
                        me.addIds(record);
                } else {
                    if (me.canSelectFolders == true) {
                        me.recursivePush(node, true);
                    }
                }
            } else {
                if (me.multiselect == false) {
                    if (me.canSelectFolders == true)
                        me.recursiveUnPush(node);
                    else
                        me.removeIds(record);
                } else
                    me.recursiveUnPush(node);
            }
        }

        //this will check every parent node that has his all children selected
        if (me.canSelectFolders == true && me.multiselect == true)
            me.checkParentNodes(node.parentNode);

        me.setRecordsValue(view, record, item, index, e, eOpts, treeCombo);
    },
    recursivePush: function (node, setIds) {
        var me = this;

        me.addRecRecord(node);
        if (setIds) {
            me.addIds(node);
        }

        node.eachChild(function (nodesingle) {
            if (nodesingle.hasChildNodes() == true) {
                me.recursivePush(nodesingle, setIds);
            } else {
                me.addRecRecord(nodesingle);
                if (setIds)
                    me.addIds(nodesingle);
            }
        });
    },
    recursiveUnPush: function (node) {
        var me = this;
        me.removeIds(node);

        node.eachChild(function (nodesingle) {
            if (nodesingle.hasChildNodes() == true) {
                me.recursiveUnPush(nodesingle);
            } else {
                me.removeIds(nodesingle);
            }
        });
    },
    addRecRecord: function (record) {
        var me = this;

        for (var i = 0, j = me.recursiveRecords.length; i < j; i++) {
            var item = me.recursiveRecords[i];
            if (item) {
                if (item.getId() == record.getId()) {
                    return;
                }
            }
        }
        me.recursiveRecords.push(record);
    },
    indexOf: function (arr, obj, start) {
        if (!Array.prototype.indexOf) {
            for (var i = (start || 0), j = arr.length; i < j; i++) {
                if (arr[i] === obj) {
                    return i;
                }
            }
            return -1;
        }
        else {
            return arr.indexOf(obj, start);
        }
    },
    getDisplayValue: function () {
        return this.displayTpl.apply(this.displayTplData);
    },
    setValue: function (valueInit) {
        if (typeof valueInit == 'undefined') {
            return;
        }

        var me = this,
                tree = this.tree,
                values = (valueInit == '') ? [] : valueInit.split(','),
                valueFin = [],
                valueNotFoundText = me.valueNotFoundText;

        var inputEl = me.inputEl;

        if (tree.store.isLoading()) {
            me.afterLoadSetValue = valueInit;
        }

        if (inputEl && me.emptyText && !Ext.isEmpty(values)) {
            inputEl.removeCls(me.emptyCls);
        }

        if (tree == false) {
            return false;
        }

        var node = tree.getRootNode();
        if (node == null) {
            return false;
        }

        me.recursiveRecords = [];
        me.recursivePush(node, false);

        me.records = [];
        Ext.each(me.recursiveRecords, function (record) {
            var id = record.get(me.valueField),
            //index = values.indexOf('' + id);
                    index = me.indexOf(values, '' + id);

            if (me.multiselect == true) {
                record.set('checked', false);
            }

            if (index != -1) {
                valueFin.push(record.get(me.displayField));
                if (me.multiselect == true)
                    record.set('checked', true);
                me.addRecord(record);
            }
        });

        me.value = valueInit;
        me.displayTplData = valueFin; //store for getDisplayValue method
        //me.setRawValue(valueFin.join(', '));
        me.setRawValue(me.getDisplayValue());

        if (valueFin.length == 0 && valueNotFoundText) {
            me.displayTplData = [valueNotFoundText];
            //me.setRawValue(valueNotFoundText);
            me.setRawValue(me.getDisplayValue());
        }

        me.checkChange();
        me.refreshEmptyText();
        return me;
    },
    checkParentNodes: function (node) {
        if (node == null)
            return;

        var me = this,
                checkedAll = true;

        node.eachChild(function (nodesingle) {
            var id = nodesingle.getId(),
            //index = me.ids.indexOf('' + id);
                    index = me.indexOf(me.ids, '' + id);

            if (index == -1)
                checkedAll = false;
        });

        if (checkedAll == true) {
            me.addIds(node);
            me.checkParentNodes(node.parentNode);
        } else {
            me.removeIds(node);
            me.checkParentNodes(node.parentNode);
        }
        me.setValue(me.ids.join(","));
    },
    addIds: function (record) {
        var me = this;

        if (me.indexOf(me.ids, '' + record.getId()) == -1) {
            me.ids.push('' + record.get(me.valueField));
        }
    },
    initValue: function () {
        if (this.value != null && this.value != undefined) {
            this.ids = this.value.split(",");
        }
        this.setValue(this.value);
        if (this.tree.getRootNode().get('checked') != null)
            this.checkNodes(this.tree.getRootNode());
    },
    // Recursive function to check if parent nodes should be checked, because their children are, but the parents are not in the initialValue
    checkNodes: function (node) {
        for (var i = 0; i < node.childNodes.length; i++) {
            var child = node.childNodes[i];
            if (child.isLeaf()) {
                this.checkParentNodes(child.parentNode);
            } else {
                this.checkNodes(child);
            }
        }
    },
    //    getValue: function () {
    //        return this.value;
    //    },
    getValue: function () {
        // If the user has not changed the raw field value since a value was selected from the list,
        // then return the structured value from the selection. If the raw field value is different
        // than what would be displayed due to selection, return that raw value.
        var me = this,
            picker = me.picker,
            rawValue = me.getRawValue(), //current value of text field
            value = me.value; //stored value from last selection or setValue() call

        if (me.getDisplayValue() !== rawValue) {
            value = rawValue;
            me.value = me.displayTplData = me.valueModels = null;
            if (picker) {
                me.ignoreSelection++;
                picker.getSelectionModel().deselectAll();
                me.ignoreSelection--;
            }
        }

        return value;
    },

    getSubmitValue: function () {
        var me = this;
        var ids = me.value.split(",");
        for (var i = ids.length; i >= 0; i--) {
            if (ids[i] == 'NaN' || ids[i] % 1 !== 0) {
                ids.splice(i, 1);
            }
        }
        var returnValue = [];
        for (var j = 0; j < ids.length; j++) {
            returnValue.push(parseInt(ids[j]));
        }
        return returnValue;
    },
    removeIds: function (record) {
        var me = this,
        //index = me.ids.indexOf('' + record.getId());
            index = me.indexOf(me.ids, '' + record.getId());

        if (index != -1) {
            me.ids.splice(index, 1);
        }
    },
    addRecord: function (record) {
        var me = this;

        for (var i = 0, j = me.records.length; i < j; i++) {
            var item = me.records[i];
            if (item) {
                if (item.getId() == record.getId())
                    return;
            }
        }
        me.records.push(record);
    },
    removeRecord: function (record) {
        var me = this;


        for (var i = 0, j = me.records.length; i < j; i++) {
            var item = me.records[i];
            if (item && item.getId() == record.getId())
                delete (me.records[i]);
        }
    },
    fixIds: function () {
        var me = this;

        for (var i = me.ids.length; i >= 0; i--) {
            if (me.ids[i] == 'NaN')
                me.ids.splice(i, 1);
        }
    },
    setRecordsValue: function (view, record, item, index, e, eOpts, treeCombo) {
        var me = treeCombo;
        me.fixIds();
        me.setValue(me.ids.join(','));
        me.fireEvent('itemclick', me, record, item, index, e, eOpts, me.records, me.ids);

        if (me.multiselect == false) {
            //me.onTriggerClick();
            if (!me.readOnly && !me.disabled) {
                if (me.isExpanded) {
                    me.collapse();
                }
                me.inputEl.focus();
            }
        }
    },
    clearFilter: function () {
        var me = this;
        me.filterField.clearFilter();
    }
});