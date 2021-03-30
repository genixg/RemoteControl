Ext.Loader.setConfig({ enabled: true });
Ext.Loader.setPath('Ext.ux.tree', '/Website/Scripts/ext652.js/build/ux/tree');
Ext.Loader.setPath('Ext.ux', '/Website/Scripts/ext652.js/build/ux');
//Ext.require(['Ext.ux.CheckColumn']);

window.ManagePanel = function () {
	this.init();
};


ManagePanel.prototype.init = function () {
	var me = this;
	me.msg = new message(this);

	me.PModel = Ext.define('EModel',
		{
			extend: 'Ext.data.Model',
			idProperty: 'id',
			identifier: 'negative',
			fields:
				[
					{ name: 'id', type: 'int' },
					{ name: 'name', type: 'string' },
					{ name: 'position', type: 'string' },
					{ name: 'status', type: 'string' },
					{ name: 'statusTill', type: 'string' },
					{ name: 'checksTotal', type: 'int' },
					{ name: 'checksNotSent', type: 'int' },
					{ name: 'checksNotTyped', type: 'int' },
					{ name: 'maxTimePeriod', type: 'int' },
					{ name: 'checksHtml', type: 'string' },
					{ name: 'checksInfoHtml', type: 'string' },
					{ name: 'timeHoursDifToServer', type: 'int' },
					{ name: 'isControlled', type: 'bool' },
					{ name: 'workingTimeStart', type: 'string' },
					{ name: 'workingTimeEnd', type: 'string' },
					{ name: 'workingTime', type: 'string' }
				]
		}
	);

	me.DModel = Ext.define('DModel',
		{
			extend: 'Ext.data.Model',
			//idProperty: 'id',
			identifier: 'negative',
			fields:
				[
					{ name: 'id', type: 'int' },
					{ name: 'name', type: 'string' },
					{ name: 'root', type: 'bool' },
					{ name: 'parentID', type: 'string' },
					{ name: 'guid', type: 'string' },
					{ name: 'countEmployees', type: 'int' },
					{ name: 'iconСls', type: 'string', defaultValue: 'treenode-no-icon' }
					//,{ name: 'ParentID', type: 'int' }
				]
		}
	);

	me.DepartmentsTreeStore = Ext.create('Ext.data.TreeStore', {
		model: 'DModel',
		filterer: 'bottomup',
		storeId: 'DepartmentsTreeStore',
		autoLoad: true,
		root: { expanded: true, loaded: true, root:true, name: 'Все подразделения', id: 0, editable: false },
		//autoSync: true,
		proxy:
		{
			type: 'ajax',
			async: true,
			url: '/api/Departments',
			timeout: 3600000,
			extraParams:
			{
				partnerid: ''
			},
			reader: { type: 'json', rootProperty: 'children' },
			writer: { type: 'json', writeAllFields: true, allowSingle: false },
			api: {
				create: '/api/Departments/Create',
				update: '/api/Departments/Update',
				read: '/api/Departments/',
				destroy: '/api/Departments/Remove'
			},
			listeners: {
				exception: function (proxy, response, operation, eOpts) {
					var message = response.statusText;
					var json = proxy.reader.jsonData;
					if (json && !json.success && json.message) {
						message = json.message;
					}
					Ext.Msg.alert('Ошибка', "Получение подразделений: " + message, Ext.emptyFn);
				}
			}
		},
		filters: [
			function (item) {
				return item.get('root') === true || item.get('countEmployees') !== 0;
			}
		],
		listeners:
		{
			beforeload: function () {
				me.DepartmentsTreeGrid.mask();
				me.DepartmentsTreeGrid.setLoading("Загрузка...");
			},
			load: function (cur, records, operation, success) {
				me.DepartmentsTreeGrid.unmask();
				me.DepartmentsTreeGrid.setLoading(false);
				var sum = 0;
				for (var i = 0; i < records.length; i++)
					sum += records[i].get('countEmployees');
				cur.root.set('countEmployees', sum, { dirty: false });
				//me.DepartmentsTreeStore.getRoot().set("DepartmentName", me.currentRecord.get('FullName'), { dirty: false });

			}
		}
	});

	me.EmployeesStore = Ext.create('Ext.data.Store', {
		model: 'EModel',
		storeId: 'EmployeesStore',
		autoLoad: false,
		proxy:
		{
			type: 'ajax',
			async: true,
			url: '/api/Employees',
			timeout: 3600000,
			extraParams:
			{
				departmentID: 0,
				dateEnd: null,
				dateStart: null
			},
			reader: { type: 'json', rootProperty: 'children' },
			writer: { type: 'json', writeAllFields: true, allowSingle: false },
			api: {
				create: '/api/Employees/',
				update: '/api/Employees/',
				read: '/api/Employees/',
				destroy: '/api/Employees/'
			},
			listeners: {
				exception: function (proxy, response, operation, eOpts) {
					var message = response.statusText;
					var json = proxy.reader.jsonData;
					if (json && !json.success && json.message) {
						message = json.message;
					}
					Ext.Msg.alert('Ошибка', "Получение подразделений: " + message, Ext.emptyFn);
				}
			}
		},
		listeners:
		{
			beforeload: function (store) {
				store.proxy.extraParams.dateStart = Ext.getCmp('dateStart').getSubmitValue();
				store.proxy.extraParams.dateEnd = Ext.getCmp('dateEnd').getSubmitValue();
			},
			load: function (records, operation, success) {
				//me.DepartmentsTreeStore.getRoot().set("DepartmentName", me.currentRecord.get('FullName'), { dirty: false });
			}
		}
	});

	me.FilterTreeLocal = function (filterValue) {
		me.SearchFieldValue = filterValue;
		me.DepartmentsTreeGrid.setLoading(true);
		me.DepartmentsTreeGrid.getStore().clearFilter();

		var _regexp = new RegExp(filterValue, 'gi');
		me.DepartmentsTreeGrid.getStore().filterBy(function (n) {
			return n.get('name').match(_regexp)
		});
		me.DepartmentsTreeGrid.setLoading(false);
	};

	me.DepartmentsTreeGrid = Ext.create('Ext.tree.Panel',
		{
			margin: '56 0 0 0',
			region: 'west',
			rootVisible: true,
			width: 300,
			title: 'Подразделения',
			store: me.DepartmentsTreeStore,
			columns: {
				items: [{
					flex: 1,
					xtype: 'treecolumn',
					dataIndex: "name",
					text: 'Подразделение'
				}, {
					dataIndex: 'countEmployees',
					text: 'Сотрудников',
					width: 30
				}]
			},
			dockedItems: [{
				xtype: 'toolbar',
				dock: 'top',
				items: [{
					text: 'Импорт',
					handler: function () {
						me.objectImportWindow.show();
					}
				}]
			}, {
				xtype: 'toolbar',
				dock: 'top',
				items: [{
					xtype: 'textfield', id: 'searchfield', labelWidth: 40,
					enableKeyEvents: true,
					emptyText: 'Поиск по наименованию',
					flex: 1,
					listeners: {
						'keypress': function (field, event) {
							if (event.getKey() == event.ENTER) {
								me.FilterTreeLocal(Ext.getCmp("searchfield").getValue());
							}
						}
					}
				}, {
					xtype: 'button', id: 'btnSearch',
					icon: '/img/search_new.png',
					tooltip: 'Поиск по наименованию',
					handler: function () {
						me.FilterTreeLocal(Ext.getCmp("searchfield").getValue());
					}
				}]

			}],
			listeners: {
				select: function (s, record, index, eOpts) {
					//me.SelectFolder(record);
					me.EmployeesStore.proxy.extraParams.departmentID = record.get('id');
					me.EmployeesStore.load();
				}
			},
			plugins: []
		});

	me.employeeContextMenu = new Ext.menu.Menu({
		items: [{
			id: 'do-something',
			text: 'Do something',
			handler: function (a, b, c) {
				alert("123123");
			}
		}],
		listeners: {
			itemclick: function (item) {
				switch (item.id) {
					case 'do-something':
						break;
				}
			}
		}
	});

	me.EmployeesGrid = Ext.create('Ext.grid.Panel',
		{
			margin: '56 0 0 0',
			region: 'center',
			flex: 1,
			title: 'Сотрудники',
			store: me.EmployeesStore,
			viewConfig: {
				getRowClass: function (record, index, rowParams, store) {
					return record.get('isControlled') === false ? 'notcontrolled' : '';
				}
			},
			columns: {
				items: [{
					width: 40,
					text: "УИд",
					dataIndex: "id"
				},{
					flex: 2,
					text: "ФИО",
					dataIndex: "name"
				}, {
					text: 'Должность',
					flex: 1,
					dataIndex: 'position'
				}, {
					text: 'Проверок всего',
					dataIndex: 'checksTotal'
				}, {
					text: 'Запланировано',
					dataIndex: 'checksNotSent'
				}, {
					text: 'Просрочено',
					dataIndex: 'checksNotTyped'
				}, {
					text: 'Макс.отклик (мин)',
					dataIndex: 'maxTimePeriod'
				}, {
					text: 'Раб.день',
					dataIndex: 'workingTime',
				}, {
					flex: 2,
					text: "Проверки",
					dataIndex: "checksHtml"
				}, {
					flex: 2,
					text: "Объяснительные",
					dataIndex: "checksInfoHtml"
				}]
			},
			dockedItems: [{
				xtype: 'toolbar',
				dock: 'top',
				items: [{
					xtype: 'datefield',
					id: 'dateStart',
					format: 'd.m.Y',
					submitFormat: 'd.m.Y',
					value: (new Date()),
					label: 'Дата начала',
					width: 190,
					labelWidth: 80,
					fieldLabel: 'Дата начала',
					listeners: {
						change: function (a,b,c,d) {
							me.EmployeesStore.load();
						}
					}
				}, {
					xtype: 'datefield',
					format: 'd.m.Y',
					submitFormat: 'd.m.Y',
					fieldLabel: 'Дата окончания',
					id: 'dateEnd',
					margin: '0 0 0 10',
					width: 200,
					labelWidth: 100,
					value: (new Date()),
					listeners: {
						change: function (a, b, c, d) {
							me.EmployeesStore.load();
						}
					}
				}, {
					xtype: 'button',
					icon: '/img/xls.png',
					margin: '0 0 0 10',
					id: 'btnMonitoringExport',
					handler: function () {
						me.ExportWindow.show();
					}
				}]
			}],
			listeners: {
				select: function (s, record, index, eOpts) {
					//me.SelectFolder(record);
				},
				rowcontextmenu: function (grid, record, tr, rowIndex, e, eOpts) {
					var changeIsControlled = function (id, value) {
						Ext.Ajax.request({
							method: 'GET',
							url: '/api/Employees/updateIsControlled',
							params: { id: id, value: value },
							success: function (response) {
								grid.getStore().load();
							},
							failure: function () {
								console.log("Error update value IsControlled");
							}
						});
					};
					var isControlled = record.get('isControlled');
					var menu_grid = new Ext.menu.Menu({items:[
						{ text: 'Отключить проверки', disabled: !isControlled, handler: function () { changeIsControlled(record.get('id'), false); } },
						{ text: 'Включить проверки', disabled: isControlled, handler: function () { changeIsControlled(record.get('id'), true); } }
					]});
					e.stopEvent();
					menu_grid.showAt(e.getXY());
				}
			},
			plugins: []
		});

	me.objectImportWindow = new Ext.Window(
		{
			constrainHeader: true,
			width: 600,
			height: 100,
			title: 'Импорт сотрудников из 1С',
			closeAction: 'close',
			modal: true,
			border: 0,
			layout: { type: 'vbox', align: 'stretch' },
			items: [{
				defaultType: 'textfield',
				xtype: "form",
				id: "importform",
				margins: '5 5 0 5',
				width: 600,
				url: '/api/Departments/import/?type=employees',
				layout: { type: 'vbox', align: 'stretch' },
				items: [{
						xtype: 'filefield',
						margin: '10 5 10 5',
						id: 'filestruct',
						name: 'filestruct',
						labelWidth: 200,
						buttonText: 'Выбрать...',
						fieldLabel: 'Выберите файл',
						allowBlank: false,
						paddings: '5 5 10 5'
					}
				],
				listeners: {
					actioncomplete: function (m, action, eOpts) {
						me.objectImportWindow.setLoading(false);
						var message = '';
						var json = eval('(' + action.response.responseText + ')');
						if (json && json.message) {
							message = json.message;
						}
						if (json && json.success) {
							me.msg.showPopup(CMessageType.INFO, message, EMessageType.INFO);
							me.StorePartners.load();
							me.objectImportWindow.close();
						} else if (json && !json.success) {
							me.msg.showPopup(CMessageType.ERROR, message, EMessageType.ERROR);
						}
					},
					actionfailed: function (m, action, eOpts) {
						me.objectImportWindow.setLoading(false);
						me.msg.showPopup(CMessageType.ERROR, action.response.responseText, EMessageType.ERROR);
					}
				}
			}
			],
			buttons:
				[
					{
						text: 'Загрузить',
						id: 'importobjects',
						handler: function () {
							me.objectImportWindow.setLoading(true);
							Ext.getCmp('importform').submit();
						}
					}
				]
		});

	me.ExportWindow = new Ext.Window(
		{
			constrainHeader: true,
			width: 600,
			height: 220,
			title: 'Мониторинг работы сотрудников',
			closeAction: 'close',
			modal: true,
			border: 0,
			layout: { type: 'vbox', align: 'stretch' },
			items: [{
				defaultType: 'textfield',
				xtype: "form",
				id: "exportform",
				margins: '5 5 0 5',
				bodyPadding: 5,
				border: 0,
				width: 600,
				url: '/api/Export/',
				layout: { type: 'vbox', align: 'stretch' },
				items: [{
					xtype: 'combobox',
					fieldLabel: 'Тип отчета',
					store: Ext.create('Ext.data.Store', {
						fields: ['type', 'name'],
						data: [
							{ "type": "daily", "name": "За первый выбранный день", "comment": "В отчет попадают работники, которые в указанный день работали удаленно и ввели код с опозданием более указанного времени (или не ввели его вовсе). " +
								"Перечень сотрудников выводится в порядке убывания времени опоздания, при этом вначале указываются работники, которые не ответили на сообщение. " +
								"Отчет может выводиться для конкретного подразделения или группироваться по подразделениям." },
							{ "type": "period", "name": "За период", "comment": "В отчет попадают работники, которые опоздали с вводом ответа более указанного периода времени более чем заданное количество дней (по умолчанию - 1)." +
								"Для работника указывается количество дней, в которых он не уложился в заданное время.Отчет может выводиться для конкретного подразделения или группироваться " +
								"по подразделениям.В отчете работники сортируются в обратном порядке по количеству опозданий за заданный период" }
						]
					}),
					listeners: {
						afterrender: function (cmb) {
							setTimeout(function () {
								Ext.getCmp('lblExportType').setHtml(cmb.store.byValue.map[cmb.value].data.comment);
								Ext.getCmp('maxDays').setVisible(cmb.value == "period");
							}, 100);
						},
						change: function (cmb, newValue) {
							Ext.getCmp('lblExportType').setHtml(cmb.store.byValue.map[newValue].data.comment);
							Ext.getCmp('maxDays').setVisible(newValue == "period");
						}
					},
					queryMode: 'local',
					displayField: 'name',
					valueField: 'type',
					value: "daily",
					name: 'type',
					id: 'reportType'
				}, {
					xtype: 'label',
					id: 'lblExportType',
					cls: 'lblExportType'
				}, {
					xtype: 'numberfield',
					id: 'maxLatency',
					fieldLabel: 'Максимально допустимая задержка (в минутах)',
					labelWidth: 520,
					value: 60,
					minValue: 0
				}, {
					xtype: 'numberfield',
					id: 'maxDays',
					fieldLabel: 'Число дней опоздания от',
					labelWidth: 520,
					value: 1,
					minValue: 1,
					visible: false
				}],
				listeners: {
					actioncomplete: function (m, action, eOpts) {
						me.ExportWindow.setLoading(false);
					},
					actionfailed: function (m, action, eOpts) {
						me.ExportWindow.setLoading(false);
						//me.msg.showPopup(CMessageType.ERROR, action.response.responseText, EMessageType.ERROR);
					}
				}
			}
			],
			buttons:
				[
					{
						text: 'Выгрузить',
						id: 'exportReport',
						handler: function () {
							window.location = '/api/Export/?type=' + Ext.getCmp('reportType').getValue()
								+ '&reportDateStart=' + Ext.getCmp('dateStart').getSubmitValue()
								+ '&reportDateEnd=' + Ext.getCmp('dateEnd').getSubmitValue()
								+ '&departmentID=' + me.EmployeesStore.proxy.extraParams.departmentID
								+ '&maxLatency=' + Ext.getCmp('maxLatency').getValue()
								+ '&maxDays=' + Ext.getCmp('maxDays').getValue();
						}
					}
				]
		});
};

ManagePanel.prototype.createPanel = function (id) {
	var me = this;
	Ext.create('Ext.Viewport',
		{
			defaults: {
				frame: false,
				split: true
			},
			layout: 'border',
			items: [
				me.DepartmentsTreeGrid, me.EmployeesGrid
			]
		});
};
