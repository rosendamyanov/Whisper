// --- DATA INTERFACES ---

export interface User {
    id: string,
    name: string,
    avatar: string,
    status: "online" | "away" | "dnd" | "offline"
}

export interface Member extends User {
    role?: "admin" | "member";
}