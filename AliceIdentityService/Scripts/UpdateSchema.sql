ALTER TABLE "OpenIddictApplications" RENAME COLUMN "Type" TO "ClientType";
ALTER TABLE "OpenIddictApplications" ADD COLUMN "ApplicationType" character varying(50);
ALTER TABLE "OpenIddictApplications" ADD COLUMN "JsonWebKeySet" text;
ALTER TABLE "OpenIddictApplications" ADD COLUMN "Settings" text;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20240527232451_InitialSchema', '8.0.5');
