fromAll()
.when({
    'Scheduled': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    },
    'PlayerRemovedFromTheBench': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    },
    'Cancelled': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    },
    'Closed': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    },
    'StartDateChanged': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    },
    'LocationChanged': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    },
    'MaxPlayerChanged': function (s, e) {
        linkTo('emailGamesProjections', e);
        return s;
    }
})