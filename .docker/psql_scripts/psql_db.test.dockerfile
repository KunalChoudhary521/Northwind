FROM postgres:12

COPY ./.docker/psql_scripts/1_psql_migrations.sql ./docker-entrypoint-initdb.d/

COPY ./.docker/psql_scripts/2_seedData.sql ./docker-entrypoint-initdb.d/

EXPOSE 5432
CMD ["postgres"]