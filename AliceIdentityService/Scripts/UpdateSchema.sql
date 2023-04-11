ALTER TABLE "AspNetUsers" ADD COLUMN "CreationTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;

-- Create indexes for prefix search of persons
CREATE INDEX ON "AspNetUsers" (lower("FirstName") varchar_pattern_ops);
CREATE INDEX ON "AspNetUsers" (lower("LastName") varchar_pattern_ops);
CREATE INDEX ON "AspNetUsers" (lower("FirstName" || ' ' || "LastName") varchar_pattern_ops);
CREATE INDEX ON "AspNetUsers" (lower("LastName" || ', ' || "FirstName") varchar_pattern_ops);
CREATE INDEX ON "AspNetUsers" (lower("Email") varchar_pattern_ops);

-- The first argument is the pattern, e.g. 'John%', and the second argument is the max number of results.
CREATE OR REPLACE FUNCTION "SearchUsers"(varchar, integer DEFAULT NULL)
RETURNS SETOF "AspNetUsers" AS $$
    SELECT * FROM "AspNetUsers" WHERE lower("FirstName") LIKE $1
        OR lower("LastName") LIKE $1
        OR lower("FirstName" || ' ' || "LastName") LIKE $1
        OR lower("LastName" || ', ' || "FirstName") LIKE $1
        OR lower("Email") LIKE $1
        ORDER BY "FirstName", "LastName" asc
        LIMIT $2;
$$ LANGUAGE sql;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20230411054509_InitialSchema', '7.0.4');
