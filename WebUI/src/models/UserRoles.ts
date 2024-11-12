export const UserRoles = {
    admin: "admin",
} as const;

export type UserRole = (typeof UserRoles)[keyof typeof UserRoles];