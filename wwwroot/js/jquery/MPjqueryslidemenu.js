var MPSlideMenu = function() {
}

// активныи элемент меню
MPSlideMenu.prototype.activeItem = null

// отображение потомков
MPSlideMenu.prototype.onShowChild = function(aElem) {
    $(aElem).addClass("Collapse");
    $("ul", aElem).hide();
    var $targetul=$(aElem).children("ul:eq(0)")
    aElem._offsets={left:$(aElem).offset().left, top:$(aElem).offset().top}
    var menuleft=aElem.istopheader? 0 : aElem._dimensions.w
    menuleft=(aElem._offsets.left+menuleft+aElem._dimensions.subulw>$(window).width())? (aElem.istopheader? -aElem._dimensions.subulw+aElem._dimensions.w : -aElem._dimensions.w) : menuleft
    if ($targetul.queue().length<=1) //if 1 or less queued animations
        $targetul.css({left:menuleft+"px", width:aElem._dimensions.subulw+'px'}).slideDown(200)
}

// скрытие потомков
MPSlideMenu.prototype.onHideChild = function(aElem) {
    $(aElem).removeClass("Collapse");
    var $targetul=$(aElem).children("ul:eq(0)")
    $targetul.slideUp(100)
}

// скрытие всего
MPSlideMenu.prototype.onHideAll = function(aMenuID) {
    var $mainmenu=$("#"+aMenuID+">ul")

	MPSlideMenu.prototype.activeItem && MPSlideMenu.prototype.onHideChild(MPSlideMenu.prototype.activeItem);
	MPSlideMenu.prototype.activeItem = null;
	
    $mainmenu.find("ul").css({display:'none', visibility:'visible'})
}

// формирование меню
MPSlideMenu.prototype.buildmenu = function(aMenuID, arrowsvar) {
    var $mainmenu=$("#"+aMenuID+">ul")

    // поведение разворачиваемых элементов
    var $headers=$mainmenu.find("ul").parent()
    
    $headers.each(function(i){
	    var $curobj=$(this)
	    var $subul=$(this).find('ul:eq(0)')
	    this._dimensions={w:this.offsetWidth, h:this.offsetHeight, subulw:$subul.outerWidth(), subulh:$subul.outerHeight()}
	    this.istopheader=$curobj.parents("ul").length==1? true : false
	    $subul.css({top:this.istopheader? (this._dimensions.h)+"px" : 0})
	    $curobj.children("a:eq(0)").css(this.istopheader? {paddingRight: arrowsvar.down[2]} : {}).append(
		    '<img src="'+ (this.istopheader? arrowsvar.down[1] : arrowsvar.right[1])
		    +'" class="' + (this.istopheader? arrowsvar.down[0] : arrowsvar.right[0])
		    + '" style="border:0;" />'
	    )
    	
	    if (this.istopheader) {
            $curobj.click(function(){                
	            if ($(this).is(".Collapse")) {
		            MPSlideMenu.prototype.onHideChild(this);
		            MPSlideMenu.prototype.activeItem = null;
		        } else {
                    MPSlideMenu.prototype.activeItem && MPSlideMenu.prototype.onHideChild(MPSlideMenu.prototype.activeItem);
		            MPSlideMenu.prototype.activeItem = this;
                    MPSlideMenu.prototype.onShowChild(this);
		        }
    		    
                return false;
	        })
            $curobj.mouseover(function() {
                if (MPSlideMenu.prototype.activeItem != null && this != MPSlideMenu.prototype.activeItem) {
                    $(this).click();
                }
            })
	    } else {
	        $curobj.hover(function() {
	            MPSlideMenu.prototype.onShowChild(this);
	        }, function(e){
	            MPSlideMenu.prototype.onHideChild(this);
		    })
    		
		    $curobj.click(function(){ return false; })
	    }		
    })

    // поведение обрабатываемых элементов
    var $elems = $mainmenu.find("li").not($headers);
    $elems.each(function() {
        $(this).click(function() {
           MPSlideMenu.prototype.onHideAll(aMenuID);
           return false;
        });
    })
    
    MPSlideMenu.prototype.onHideAll(aMenuID);
}

// основная функция построения меню
MPSlideMenu.prototype.init = function(aMenuID) {
    var arrowimages={down:['downarrowclass', '/Website/images/separatorMenu.png', 23], right:['rightarrowclass', '/Website/images/datetimepicker/cal_forward.png']}
    
    MPSlideMenu.prototype.buildmenu(aMenuID, arrowimages);
    
	$(document).click(function() {
	    MPSlideMenu.prototype.onHideAll(aMenuID)
	});
}