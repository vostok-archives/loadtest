.PHONY: run build pull

run: build
	java -Xms16g -Xmx16g -XX:+UseG1GC -XX:MaxGCPauseMillis=200 -jar ./target/uber-kload-1.0-SNAPSHOT.jar

build: pull
	mvn clean package

pull:
	git pull