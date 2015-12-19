--sql for command Processing

BEGIN TRANSACTION;


CREATE TABLE if not exists commandProcessed (commandId text primary key not null);
CREATE TABLE if not exists eventsProcessed (projectionName text not null, eventId text not null,PRIMARY KEY (projectionName, eventId));
CREATE TABLE if not exists projections (projectionName text not null, lastCheckPoint int);
CREATE TABLE if not exists cleavage (cleavageId text not null, isOpenned int not null, version int);
CREATE TABLE if not exists team (teamId text not null,cleavageId text not null, name text not null);
CREATE TABLE if not exists teamPlayers (teamId text not null, bearId text not null);


ALTER TABLE Bears ADD COLUMN email text ;
ALTER TABLE GamesList ADD Version  text ;
ALTER TABLE Rooms ADD Version  text ;
ALTER TABLE RoomMessages ADD typeMessage  text ;
ALTER TABLE GamesBears ADD Version  text ;
ALTER TABLE Bears ADD Version  text ;
ALTER TABLE Authentication ADD Version  text ;
ALTER TABLE Users ADD Version  text ;


COMMIT;
