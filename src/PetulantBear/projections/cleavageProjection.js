fromAll()
.when({
    'CleavageProposed': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'NameTeamChanged': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'TeamJoined': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'TeamLeaved': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'PlayerSwitched': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'PlayerKickedFromTeam': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'NameChanged': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'StartDateChanged': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'LocationChanged': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    },
    'MaxPlayerChanged': function (s, e) {
        linkTo('cleavageProjection', e);
        return s;
    }
})