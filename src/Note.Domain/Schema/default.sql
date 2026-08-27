CREATE TABLE IF NOT EXISTS "application" ("id" INTEGER NOT NULL PRIMARY KEY, "external_id" TEXT, "name" TEXT, "created_at" DATETIME);
CREATE TABLE IF NOT EXISTS "application_credentials" ("id" INTEGER NOT NULL PRIMARY KEY, "application_id" INTEGER, "credential" TEXT, "created_at" DATETIME);
CREATE TABLE IF NOT EXISTS "note" ("id" INTEGER NOT NULL PRIMARY KEY, "external_id" TEXT, "title" TEXT, "text" TEXT, "created_at" DATETIME, "updated_at" DATETIME);
CREATE UNIQUE INDEX "application_external_id" ON "application" ("external_id");
CREATE UNIQUE INDEX "note_external_id" ON "note" ("external_id");
CREATE TABLE IF NOT EXISTS "hmac_nonce" ("id" INTEGER NOT NULL PRIMARY KEY, "nonce" TEXT, "created_at" DATETIME);
CREATE UNIQUE INDEX "hmac_nonce_nonce" ON "hmac_nonce" ("nonce");
