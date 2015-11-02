if [ ! -d "temp" ]
then
	mkdir temp 
fi

if [ ! -d "db" ]
then
	mkdir db
fi

rm -r db/Bear2Bear.db

sqlite3 db/Bear2Bear.db < scripts/authenticationScript.sql


