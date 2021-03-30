Ext.define('Ext.ux.tree.TreeStoreRemoteFilter', {
    extend: 'Ext.ux.tree.TreeStoreFilter652'
    , alias: 'plugin.treestoreremotefilter'

    /**
    * @cfg {String/Function} url
    * The URL to which to send the request, or a function to call which returns a URL string. The scope of the function is specified by the scope option. Defaults to the configured url.
    */
    , url: ''
    /**
    * @cfg {Number} timeout
    * The timeout in milliseconds to be used for this request. Defaults to 300000 seconds
    */
    , timeout: 300000
    /**
    * @cfg {Object/String/Function} url
    * An object containing properties which are used as parameters to the request, a url encoded string or a function to call to get either. The scope of the function is specified by the scope option.
    */
    , params: undefined

    , idProperty: 'id'

    , constructor: function (config) {
        var me = this;
        Ext.apply(me, config);
    }

    , init: function (tree) {
        var me = this;
        me.callParent(arguments);

        var store = me.store;

        //с версии 5.0.0 не обязательно, см. https://github.com/ivan-novakov/extjs-upload-widget/issues/26
//        store.addEvents({
//            'filterRemote': true,
//            'filterRemoteOnly': true,
//            'clearFilterRemote': true
//        });

        store.filterParams = {};
        Ext.apply(store.filterParams, { url: me.url, timeout: me.timeout, params: me.params });
        store.filterIdProperty = me.idProperty;
        store.filterRemote = Ext.Function.bind(me.filterRemote, store);
        store.filterFn = Ext.Function.bind(me.filterFn, store);
        store.clearFilterRemote = Ext.Function.bind(me.clearFilterRemote, store);

        store.on('load', me.onLoad, store);
    }

    , filterRemote: function () {
        var me = this;
        //store = me.store;

        //загружаем основные данные
        Ext.Ajax.request(Ext.apply({
            url: me.url,
            timeout: me.timeout,
            params: me.params,
            success: function (response) {
                if (response.responseText == "") {
                    return;
                }
                var tableData = Ext.decode(response.responseText);
                me.filterParams.filterIds = tableData.ids;
                me.filterParams.filterPathes = tableData.pathes;

                me.filterBy(me.filterFn, me);

                me.fireEvent('filterRemote', me);
                me.fireEvent('filterRemoteOnly', me);
            }
        }, me.filterParams));
    }

    , filterFn: function (item, id, scope) {
        var me = scope || this;
        var ids = me.filterParams.filterIds;
        var pathes = me.filterParams.filterPathes;

        var rec = item;
        var res = false;
        var recId = null;
        var filterStarted = false;
        do {
            recId = rec.get(me.filterIdProperty);
            if (!filterStarted && pathes[recId] || ids[recId]) {
                res = true;
                break;
            }
            //                        if (ids[recId]) {
            //                            res = true;
            //                            break;
            //                        }

            rec = rec.parentNode;
            filterStarted = true;

        } while (rec && !res);
        return res;
    }

    , clearFilterRemote: function () {
        var me = this;
        delete me.filterParams.filterIds;
        delete me.filterParams.filterPathes;

        me.clearFilter(arguments);

        me.fireEvent('clearFilterRemote', me);

        return me;
    }

    , onLoad: function (store, records, successful, operation, node, eOpts) {
        var me = this;
//        var nodeId = node.getId();
//        var snapshot = me.snapshot;
//        me.loadedOnFilter = me.loadedOnFilter || [];
        if (successful && me.isFiltered()) {
//            var loadedItem = null;
//            var item = null;
//            for (var i = 0; i < me.loadedOnFilter.length; i++) {
//                item = me.loadedOnFilter[i];
//                if (item.id == nodeId) {
//                    loadedItem = item;
//                    break;
//                }
//            }

//            var rec = null;
//            var recInSnapshot = null;
//            var recordsCopy = [];
//            for (var i = 0; i < records.length; i++) {
//                rec = records[i];
//                recordsCopy.push(me.copyNode(null, undefined, true, rec));
//                if (!me.filterFn(rec)) {
//                    records.splice(i, 1);
////                    recInSnapshot = snapshot.findChild('id', node.getId(), true);
////                    if (recInSnapshot) {
////                        recInSnapshot.appendChild(me.copyNode(null, true, rec));
////                    }
//                    rec.remove();
//                    //сбрасываем удаление узла
//                    Ext.Array.remove(me.removed, rec);
//                    i--;
//                }
//            }

//            if (loadedItem) {
//                loadedItem.records = recordsCopy; //records;
//            }
//            else {
//                me.loadedOnFilter.push({ id: nodeId, records: recordsCopy/*records*/ });
//            }

            me.suspendEvents();
            var rec = null;
            for (var i = 0; i < records.length; i++) {
                rec = records[i];
                if (!me.filterFn(rec)) {
//                    records.splice(i, 1);

//                    rec.remove();
//                    //сбрасываем удаление узла
//                    Ext.Array.remove(me.removed, rec);

                    //i--;

                    if (!rec.isRoot()) {
                        rec.set('visible', false);
                    }
                }
            }
            me.resumeEvents(true);
            me.fireEvent('refresh', me);

            me.fireEvent('filterRemote', me, records);
        }
    }
});