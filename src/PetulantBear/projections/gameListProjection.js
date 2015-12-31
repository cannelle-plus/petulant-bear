fromAll()
.when({
    'Scheduled': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'Joined': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'Abandonned': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'Cancelled': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'Closed': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'PlayerKicked': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'NameChanged': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'StartDateChanged': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'LocationChanged': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    },
    'MaxPlayerChanged': function (s, e) {
        linkTo('gameListProjection', e);
        return s;
    }
})