--sql for command Processing

BEGIN TRANSACTION;


CREATE TABLE if not exists commandProcessed (commandId text primary key not null);
CREATE TABLE if not exists eventsProcessed (projectionName text not null, eventId text not null,PRIMARY KEY (projectionName, eventId));
CREATE TABLE if not exists projections (projectionName text not null, lastCheckPoint int);
CREATE TABLE if not exists cleaveage (cleaveageId text not null, gameId text not null, isOpenned int not null, version int);
CREATE TABLE if not exists team (teamId text not null,cleaveageId text not null, name text not null);
CREATE TABLE if not exists teamPlayers (teamId text not null, bearId text not null);

CREATE TABLE if not exists gameFinished (gameId text primary key not null,teamAId text not null, teamAScore int not null, teamBId text not null, teamBScore int not null, version int);


CREATE TABLE if not exists notifications (notificationId text not null, notificationType text not null, eventId text not null);
--email part
CREATE TABLE if not exists emailToSend (notificationId text not null, subject text not null, body text not null, recipient text not null, nbAttempt int not null);
CREATE TABLE if not exists emailSent (notificationId text not null, subject text not null, body text not null, recipient text not null, nbAttempt int not null);
CREATE TABLE if not exists deadQueue (notificationId text not null, subject text not null, body text not null, recipient text not null, nbAttempt int not null);

ALTER TABLE Bears ADD COLUMN email text ;
ALTER TABLE GamesList ADD Version  text ;
ALTER TABLE Rooms ADD Version  text ;
ALTER TABLE RoomMessages ADD typeMessage  text ;
ALTER TABLE GamesBears ADD Version  text ;
ALTER TABLE Bears ADD Version  text ;
ALTER TABLE Authentication ADD Version  text ;
ALTER TABLE Users ADD Version  text ;


COMMIT;
