$(function () {
    bindStyle();

    /**
     * 根据控制器名称设置nav样式
     */
    function bindStyle() {
        var $li = $('.nav.navbar-nav>li>a[href="/' + $('[name=hidCName]').val() + '"]').parent();
        $li.siblings().removeClass('active');
        $li.addClass('active');
    }
});
