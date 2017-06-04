.PHONY: run build pull

run: build
	java -Xms4g -Xmx4g -jar ./target/uber-kload-1.0-SNAPSHOT.jar

build: pull
	mvn clean package

pull:
	git pull