fromAll()
.when({
    'CleaveageProposed': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'NameTeamChanged': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'TeamJoined': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'TeamLeaved': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'PlayerSwitched': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'PlayerKickedFromTeam': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'NameChanged': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'StartDateChanged': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'LocationChanged': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    },
    'MaxPlayerChanged': function (s, e) {
        linkTo('cleaveageProjection', e);
        return s;
    }
})