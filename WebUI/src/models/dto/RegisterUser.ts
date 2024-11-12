export type RegisterUserRequest = {
    username: string;
    email: string;
    password: string;
};

export type RegisterUserFormErrors = {
    username?: string;
    email?: string;
    password?: string;
};

export type RegisterUserForm = {
    confirmPassword: string;
} & RegisterUserRequest;
