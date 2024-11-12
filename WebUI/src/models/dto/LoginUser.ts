export type LoginUserForm = {
    email: string;
    password: string;
}

export type LoginUserFormErrors = {
    email?: string;
    password?: string;
}

export type LoginUserRequest = LoginUserForm;