CREATE TABLE Users (
                       userId UUID PRIMARY KEY,
                       username VARCHAR(255) UNIQUE NOT NULL,
                       passwordHash VARCHAR(255) NOT NULL,
                       email VARCHAR(255) UNIQUE NOT NULL,
                       createdAt TIMESTAMP NOT NULL,
                       updatedAt TIMESTAMP NOT NULL
);

CREATE TABLE Roles (
                       roleId SERIAL PRIMARY KEY, -- Use SERIAL for auto-incrementing ID
                       roleName VARCHAR(255) UNIQUE NOT NULL,
                       createdAt TIMESTAMP NOT NULL,
                       updatedAt TIMESTAMP NOT NULL
);

CREATE TABLE UserRoles (
                           userId UUID REFERENCES Users(userId),
                           roleId INT REFERENCES Roles(roleId),
                           createdAt TIMESTAMP NOT NULL,
                           updatedAt TIMESTAMP NOT NULL,
                           PRIMARY KEY (userId, roleId)
);

INSERT INTO Roles (roleName, createdAt, updatedAt)
VALUES
    ('user', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('admin', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

GRANT SELECT, INSERT, UPDATE, DELETE ON Users TO "UserManagementServiceUser";
GRANT SELECT, INSERT, UPDATE, DELETE ON userroles TO "UserManagementServiceUser";
GRANT SELECT, INSERT, UPDATE, DELETE ON roles TO "UserManagementServiceUser";

