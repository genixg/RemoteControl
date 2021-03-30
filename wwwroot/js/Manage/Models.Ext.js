Ext.define('Manage.model.Employee',
	{
		extend: 'Ext.data.Model',
		idProperty: 'Id',
		identifier: 'negative',
		fields:
			[
				{ name: 'Id', type: 'int' },
				{ name: 'Name', type: 'string' },
				{ name: 'ParentID', type: 'string' },
				{ name: 'DrawingName', type: 'string' },
				{ name: 'cls', type: 'string' },
				{ name: 'HasDrawings', type: 'bool' },
				{ name: 'iconCls', type: 'string', defaultValue: 'treenode-no-icon' }
				//,{ name: 'ParentID', type: 'int' }
			]
	}
);