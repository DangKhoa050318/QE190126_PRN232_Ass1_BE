-- ============================================================
-- PRN232 Assignment 2 — Authentication migration (PostgreSQL)
-- Run AFTER TaskManagementDB_Postgres.sql. Safe to run more than once:
-- existing tables, columns, constraints and the admin row are left untouched.
--
-- Adds:
--   * SystemAccount  — user accounts (passwords stored as BCrypt hashes only)
--   * RefreshToken   — refresh tokens (bonus); only a SHA-256 hash of each token is stored
--   * Task.CreatedByID / Task.UpdatedByID — audit fields (bonus), also used to block
--     deleting an account that has created tasks
--   * The Admin account. Admin cannot be created through POST /api/auth/register,
--     which only creates Staff accounts.
-- ============================================================

-- ============================================================
-- TABLE: SystemAccount
-- ============================================================
CREATE TABLE IF NOT EXISTS "SystemAccount" (
    "AccountID"     SERIAL          PRIMARY KEY,
    "FullName"      VARCHAR(100)    NOT NULL,
    "Email"         VARCHAR(255)    NOT NULL,
    "PasswordHash"  VARCHAR(255)    NOT NULL,
    "Role"          SMALLINT        NOT NULL DEFAULT 0,
    -- Role: 0 = Staff, 1 = Admin
    "CreatedDate"   TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "UQ_SystemAccount_Email" UNIQUE ("Email"),
    CONSTRAINT "CK_SystemAccount_Role"  CHECK ("Role" IN (0, 1))
);

-- ============================================================
-- TABLE: RefreshToken
-- ============================================================
CREATE TABLE IF NOT EXISTS "RefreshToken" (
    "RefreshTokenID" SERIAL         PRIMARY KEY,
    "AccountID"      INT            NOT NULL,
    "TokenHash"      VARCHAR(64)    NOT NULL,
    "ExpiresAt"      TIMESTAMP      NOT NULL,
    "CreatedDate"    TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "RevokedAt"      TIMESTAMP      NULL,
    CONSTRAINT "UQ_RefreshToken_TokenHash" UNIQUE ("TokenHash"),
    CONSTRAINT "FK_RefreshToken_Account"   FOREIGN KEY ("AccountID")
        REFERENCES "SystemAccount" ("AccountID") ON DELETE CASCADE
);

-- ============================================================
-- Task audit fields
-- ============================================================
ALTER TABLE "Task" ADD COLUMN IF NOT EXISTS "CreatedByID" INT NULL;
ALTER TABLE "Task" ADD COLUMN IF NOT EXISTS "UpdatedByID" INT NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Task_CreatedBy') THEN
        -- No ON DELETE action: an account that created tasks cannot be deleted.
        ALTER TABLE "Task" ADD CONSTRAINT "FK_Task_CreatedBy"
            FOREIGN KEY ("CreatedByID") REFERENCES "SystemAccount" ("AccountID");
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Task_UpdatedBy') THEN
        ALTER TABLE "Task" ADD CONSTRAINT "FK_Task_UpdatedBy"
            FOREIGN KEY ("UpdatedByID") REFERENCES "SystemAccount" ("AccountID") ON DELETE SET NULL;
    END IF;
END $$;

-- ============================================================
-- SEED: Admin account (password is stored as a BCrypt hash, cost 11)
-- ============================================================
INSERT INTO "SystemAccount" ("FullName", "Email", "PasswordHash", "Role") VALUES
    ('TaskTrack Admin', 'admin@tasktrack.test', '$2a$11$3Mz1P91NGHS6hkiocjBz3eefLZ9hz.GhwJDMedHALEULh0l7YePp6', 1)
ON CONFLICT ("Email") DO NOTHING;

-- ============================================================
-- Verify
-- ============================================================
SELECT "AccountID", "FullName", "Email",
       CASE "Role" WHEN 1 THEN 'Admin' ELSE 'Staff' END AS "Role"
FROM "SystemAccount"
ORDER BY "AccountID";
