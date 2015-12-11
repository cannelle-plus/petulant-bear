var bearInterval = 0;

doNothing = function(event){
	event.preventDefault();
	event.stopPropagation();
}

showSection = function(destination) {
	clearInterval(bearInterval);
	var id = '#' + destination,
		tX = -100 * $(id).index();
	$('#main > section').css('transform','translateX( ' + tX + '%)');
};

// Navigation
$('body').on('click','[data-target]',function(e){
	doNothing(e);
	showSection($(this).attr('data-target'));
	history.pushState(null, null, '#_' + $(this).attr('data-target'));	
}).on('click','a[data-modal]',function(e){
	// Modal
	doNothing(e);
	if($(this).attr('data-modal') === 'open')
		$('.overlay').show();
	else	
		$('.overlay').hide();
});	

// History
window.addEventListener("hashchange", function(e) {
  showSection(window.location.hash.substr(2));
})
if(window.location.hash.substr(2) !== null) {
	$('header h1 a').click();
}

// Loader
var loader = {
	show: function() {
        $('.loading').show();
	},
	hide: function() {
        $('.loading').hide();
	}
}