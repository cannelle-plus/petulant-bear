fromAll()
.when({
    'ScoreGiven': function (s, e) {
        linkTo('finishedGameProjection', e);
        return s;
    }
})