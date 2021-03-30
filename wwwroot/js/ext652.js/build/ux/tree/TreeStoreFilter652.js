
//ВНИМАНИЕ!!!! только локальная фильтрация!!! для удаленной фильтрации ничего не проверялось и не правилось!!!
Ext.define('Ext.ux.tree.TreeStoreFilter652', {
    extend: 'Ext.AbstractPlugin'
    , alias: 'plugin.treestorefilter652'

    //, collapseOnClear: true                                                 // collapse all nodes when clearing/resetting the filter
    //, allowParentFolders: false                                             // allow nodes not designated as 'leaf' (and their child items) to  be matched by the filter
    , allNodesReal: false//если все узлы реальные, то делаем в snapshot все узлы реальными (т.к. узлы без id создаются как phantom)

    , init: function (tree) {
        var me = this;
        me.tree = tree;

        var store = me.store = tree.getStore(); //.treeStore;

        //с версии 5.0.0 не обязательно, см. https://github.com/ivan-novakov/extjs-upload-widget/issues/26
        //        store.addEvents({
        //            'filter': true,
        //            'clearFilter': true
        //        });

        store.filterParams = {};
        Ext.apply(store.filterParams, { isLocal: true, allNodesReal: me.allNodesReal });
        store.filterBy = Ext.Function.bind(me.filterBy, store);
        store.clearFilter = Ext.Function.bind(me.clearFilter, store);
        store.isFiltered = Ext.Function.bind(me.isFiltered, store);
        //        store.copyNode = Ext.Function.bind(me.copyNode, store);

        //        store.on('load', me.onLoad, store);

        //        store.on('update', me.onUpdate, store);
        //        store.on('move', me.onMove, store);
    }

    , filterBy: function (fn, scope) {
        var me = this,
              root;

        me.isFiltering = true;
        me.clearFilter();
        // the snapshot holds a copy of the current unfiltered tree
        root = me.getRootNode();
        //me.snapshot = me.snapshot || me.copyNode(null, undefined, true, root);

        var nodesToRemove = Ext.clone(me.byIdMap);

        var nodeId = null;
        var parentNodeId = null;
        var childNodeId = null;
        Ext.Object.each(me.byIdMap, function (key, node) {
            nodeId = node.getId() || node.internalId;
            if (fn && fn(node, nodeId, scope) && nodesToRemove.hasOwnProperty(nodeId)) {
                //node.remove();
                delete nodesToRemove[nodeId];
                var parentNode = node.parentNode;
                while (parentNode) {
                    parentNodeId = parentNode.getId() || parentNode.internalId;
                    if (nodesToRemove.hasOwnProperty(parentNodeId)) {
                        delete nodesToRemove[parentNodeId];
                    }
                    parentNode = parentNode.parentNode;
                }

                //показываем все дочерние узлы
                if (me.filterParams.isLocal) {
                    node.cascadeBy(function (childNode) {                            // iterate over its children and set them as visible
                        childNodeId = childNode.getId() || childNode.internalId;
                        if (nodesToRemove.hasOwnProperty(childNodeId)) {
                            delete nodesToRemove[childNodeId];
                        }
                    });
                }
            }
        });

        me.suspendEvents();
        Ext.Object.each(nodesToRemove, function (key, node) {
            //            node.remove();
            //            //сбрасываем удаление узла
            //            Ext.Array.remove(me.removedNodes, node);
            if (!node.isRoot()) {
                node.set('visible', false);
            }
        });
        me.resumeEvents(true);
        me.fireEvent('refresh', me);

        delete me.isFiltering;
        me._isFiltered = true;
        me.fireEvent('filter', me);
    }

    , clearFilter: function () {
        var me = this;

        if (me.isFiltered()) {
            me.suspendEvents();
            Ext.Object.each(me.byIdMap, function (key, node) {
                node.set('visible', true);
            });
            me.resumeEvents(true);
            me.fireEvent('refresh', me);
            //delete me.snapshot;
            delete me._isFiltered;
            delete me.loadedOnFilter;

            me.fireEvent('clearFilter', me);
        }

        return me;
    }

    , isFiltered: function () {
        //return !!this.snapshot;
        return this._isFiltered;
    }
});