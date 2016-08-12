$(function () {
    bindStyle();

    /**
     * 根据控制器名称设置nav样式
     */
    function bindStyle() {
        var cName = $('[name=hidCName]').val();
        if (cName === "Home") {
            cName = "";
        }
        var $li = $('.nav.navbar-nav>li>a[href="/' + cName + '"]').parent();
        $li.siblings().removeClass('active');
        $li.addClass('active');
    }
});
