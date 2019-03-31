.PHONY: run-database run-cqlsh run-console stop-database remove-database clean-database run-dev-database run-dev-api run-dev-cqlsh clean-dev-api nuke-dev help
 
CURRENT_WORK_DIRECTORY 	= $(shell pwd | egrep -o '(^|[^cygdrive])(d/|c/|e/).*')

NETWORK_NAME		= auth-api-network

DB_IMAGE			= cassandra:latest
DB_CONTAINER_NAME	= auth-cassandra
DB_CLUSTER_NAME		= auth-cluster
DB_PORT				= 9042
DB_PORT_LOCAL		= 9043
DB_VOLUME			= auth-database

API_CONTAINER_NAME	= auth-api

BASE_COMPOSE		= docker-compose.yml
DEV_COMPOSE			= docker-compose.development.yml

export DC_CONTAINER_NAME	= $(DB_CONTAINER_NAME)
export DC_VOLUME			= $(DB_VOLUME)
export DC_CLUSTER_NAME		= $(DB_CLUSTER_NAME)
export DC_PORT				= $(DB_PORT_LOCAL)

export DC_API_NAME			= $(API_CONTAINER_NAME)

run-database: ## Creating a new Container or starting an existing one.
	@echo "Starting Cassandra container {$(DB_CONTAINER_NAME)}..."	
	@docker-compose up -d > /dev/null

	@tput setaf 2
	@echo "The Cassandra Container {$(DB_CONTAINER_NAME)} is up and running."
	@echo "Port : $(DB_PORT)"
	@tput sgr0

run-cqlsh: ## Start Cassandra {CQLSH} console to query the database.
	@echo "Starting Cassandra {cqlsh} console..."
	@docker exec -it $(DB_CONTAINER_NAME) cqlsh

run-console: ## Start Cassandra console.
	@echo "Starting Cassandra console..."
	@docker exec -it $(DB_CONTAINER_NAME) bash

stop-database: ## Stop the database Container.
	@echo "Stopping the Cassandra container {$(DB_CONTAINER_NAME)}..."

	@if [ $(shell docker ps --no-trunc --quiet --filter name=^/$(DB_CONTAINER_NAME)$ | wc -l) -eq 1 ]; then \
		docker-compose stop > /dev/null; \
	fi

remove-database: stop-database ## Stop and remove the cassandra container 
	@echo "Removing the Cassandra container {$(DB_CONTAINER_NAME)}..."

	@if [ $(shell docker ps -a --no-trunc --quiet --filter name=^/$(DB_CONTAINER_NAME)$ | wc -l) -eq 1 ]; then \
		docker rm $(DB_CONTAINER_NAME) > /dev/null; \
	fi

clean-database: ## Stop and remove the conainer and its data.
	@echo "Cleaning the Cassandra container {$(DB_CONTAINER_NAME)}..."	
	@docker-compose down -v > /dev/null


run-dev-database: ## Create Cassandra Container for dev environment. Also create Volume and Network.
	@echo "Creating volumes..."
	@docker volume create $(DB_VOLUME)-dev -d local > /dev/null

	@echo "Creating newtwork..."
	@docker network create $(NETWORK_NAME) > /dev/null

	@echo "Creating the database container $(DB_CONTAINER_NAME)-dev..."
	@docker run \
		--name $(DB_CONTAINER_NAME)-dev \
		-e CASSANDRA_CLUSTER_NAME=$(DB_CLUSTER_NAME) \
		-p $(DB_PORT):9042 \
		-v $(DB_VOLUME)-dev:/var/lib/cassandra \
		-d cassandra:latest > /dev/null
	
		@docker network connect $(NETWORK_NAME) $(DB_CONTAINER_NAME)-dev

run-dev-api: clean-dev-api ## Build the API Image then Create the API container and finaly links the API with the Database
	@echo "Building API Image {$(API_CONTAINER_NAME)-image-dev}..."
	@docker build -t $(API_CONTAINER_NAME)-image-dev:latest .

	@echo "Creating API Container {$(API_CONTAINER_NAME)-dev}..."
	@docker create \
		--name $(API_CONTAINER_NAME)-dev \
		--link $(DB_CONTAINER_NAME)-dev:database \
		-p 6000:5000 \
		-e ASPNETCORE_ENVIRONMENT=Development \
		$(API_CONTAINER_NAME)-image-dev:latest

	@docker network connect $(NETWORK_NAME) $(API_CONTAINER_NAME)-dev

	@docker start $(API_CONTAINER_NAME)-dev

run-dev-cqlsh: ## Start Cassandra {CQLSH} console to query the database.
	@echo "Starting Cassandra {cqlsh} console..."
	@docker exec -it $(DB_CONTAINER_NAME)-dev cqlsh

clean-dev-api: ## Delete the API container and its image.
	@echo "Cleaning the API {$(API_CONTAINER_NAME)-dev}...."

	@if [ $(shell docker ps --no-trunc --quiet --filter name=^/$(API_CONTAINER_NAME)-dev$ | wc -l) -eq 1 ]; then \
		echo "Stopping Container {$(API_CONTAINER_NAME)-dev}..."; \
		docker stop  $(API_CONTAINER_NAME)-dev  > /dev/null; \
	fi
	
	@if [ $(shell docker ps -a --no-trunc --quiet --filter name=^/$(API_CONTAINER_NAME)-dev$ | wc -l) -eq 1 ]; then \
		echo "Deleting Container {$(API_CONTAINER_NAME)-dev}..."; \
		docker rm $(API_CONTAINER_NAME)-dev -v  > /dev/null; \
	fi
		
	@if [ $(shell  docker image ls | grep $(API_CONTAINER_NAME)-image-dev | wc -l) -eq 1 ]; then \
		echo "Deleting image {$(API_CONTAINER_NAME)-image-dev}..."; \
		docker rmi $(API_CONTAINER_NAME)-image-dev:latest; \
	fi

clean-dev-database: ## Delete the Cassandra dev and delete its volume.
	@echo "Cleaning the API {$(DB_CONTAINER_NAME)-dev}...."

	@if [ $(shell docker ps --no-trunc --quiet --filter name=^/$(DB_CONTAINER_NAME)-dev$ | wc -l) -eq 1 ]; then \
		echo "Stopping Container {$(DB_CONTAINER_NAME)-dev}..."; \
		docker stop  $(DB_CONTAINER_NAME)-dev  > /dev/null; \
	fi

	@if [ $(shell docker ps -a --no-trunc --quiet --filter name=^/$(DB_CONTAINER_NAME)-dev$ | wc -l) -eq 1 ]; then \
		echo "Deleting Container {$(DB_CONTAINER_NAME)-dev}..."; \
		docker rm $(DB_CONTAINER_NAME)-dev -v > /dev/null; \
	fi

	@if [ $(shell docker volume ls | grep $(DB_VOLUME)-dev | wc -l) -eq 1 ]; then \
		docker volume rm $(DB_VOLUME)-dev > /dev/null ; \
	fi

nuke-dev: clean-dev-api clean-dev-database ## Delete all dev containers and API image, then prune all the rest.
	@echo "Deleting all dev Containers, volumes and images..."

	@if [ $(shell  docker network ls | grep $(NETWORK_NAME) | wc -l) -eq 1 ]; then \
		echo "Deleting network {$(NETWORK_NAME)}..."; \
		docker network rm $(NETWORK_NAME) > /dev/null; \
	fi
	
	@docker image prune -f > /dev/null

help: ## Shows the Current Makefile Commands.
	@grep -E '^[a-zA-Z_-]+:.*$$' ./Makefile | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'