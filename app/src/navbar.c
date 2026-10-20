#include "navbar.h"

#include <SDL3/SDL_render.h>
#include <SDL3/SDL_stdinc.h>
#include <SDL3/SDL_video.h>

#include "android/keycodes.h"

#define BTN_COUNT 6
#define BTN_WIDTH 56.f

#define ICON_STROKE 2.6f
#define MAX_FAN_POINTS 64

enum sc_navbar_icon {
    ICON_BACK,
    ICON_HOME,
    ICON_RECENTS,
    ICON_VOLUME_DOWN,
    ICON_VOLUME_UP,
    ICON_POWER,
};

static const enum android_keycode button_keycodes[BTN_COUNT] = {
    AKEYCODE_BACK,
    AKEYCODE_HOME,
    AKEYCODE_APP_SWITCH,
    AKEYCODE_VOLUME_DOWN,
    AKEYCODE_VOLUME_UP,
    AKEYCODE_POWER,
};

// Scale to convert window points to window coordinates: SDL window
// coordinates may be physical pixels (density 1) on high-DPI displays, in
// which case the UI scale from the display must be applied.
static float
get_ui_scale(SDL_Window *window) {
    float scale = SDL_GetWindowDisplayScale(window);
    if (scale <= 0) {
        return 1.f;
    }
    float density = SDL_GetWindowPixelDensity(window);
    if (density > 0) {
        scale /= density;
    }
    return scale;
}

float
sc_navbar_get_height(SDL_Window *window) {
    return SC_NAVBAR_HEIGHT * get_ui_scale(window);
}

void
sc_navbar_init(struct sc_navbar *nb) {
    nb->hover = -1;
    nb->pressed = -1;
}

static void
get_button_rect(int window_width, int window_height, int index,
                SDL_FRect *rect) {
    float total = BTN_COUNT * BTN_WIDTH;
    float left = ((float) window_width - total) / 2.f;
    if (left >= 0) {
        rect->x = left + index * BTN_WIDTH;
        rect->w = BTN_WIDTH;
    } else {
        // Window narrower than the buttons: shrink them to fit
        float w = (float) window_width / BTN_COUNT;
        rect->x = index * w;
        rect->w = w;
    }
    rect->y = (float) window_height - SC_NAVBAR_HEIGHT;
    rect->h = SC_NAVBAR_HEIGHT;
}

static int
get_button_at(int window_width, int window_height, float x) {
    for (int i = 0; i < BTN_COUNT; ++i) {
        SDL_FRect rect;
        get_button_rect(window_width, window_height, i, &rect);
        if (x >= rect.x && x < rect.x + rect.w) {
            return i;
        }
    }
    return -1;
}

// Fill a convex polygon using a triangle fan around its centroid
static void
fill_fan(SDL_Renderer *renderer, const SDL_FPoint *points, int count,
         SDL_Color color) {
    if (count < 3 || count > MAX_FAN_POINTS) {
        return;
    }

    SDL_Vertex vertices[MAX_FAN_POINTS + 1];
    int indices[MAX_FAN_POINTS * 3];

    SDL_FColor c = {
        color.r / 255.f,
        color.g / 255.f,
        color.b / 255.f,
        color.a / 255.f,
    };

    float cx = 0;
    float cy = 0;
    for (int i = 0; i < count; ++i) {
        cx += points[i].x;
        cy += points[i].y;
    }
    cx /= count;
    cy /= count;

    vertices[0].position.x = cx;
    vertices[0].position.y = cy;
    vertices[0].color = c;
    vertices[0].tex_coord.x = 0;
    vertices[0].tex_coord.y = 0;

    for (int i = 0; i < count; ++i) {
        vertices[i + 1].position = points[i];
        vertices[i + 1].color = c;
        vertices[i + 1].tex_coord.x = 0;
        vertices[i + 1].tex_coord.y = 0;
    }

    int n = 0;
    for (int i = 0; i < count; ++i) {
        indices[n++] = 0;
        indices[n++] = i + 1;
        indices[n++] = (i + 1) % count + 1;
    }

    SDL_RenderGeometry(renderer, NULL, vertices, count + 1, indices, n);
}

static void
fill_circle(SDL_Renderer *renderer, float cx, float cy, float radius,
            SDL_Color color) {
    const int segments = 20;
    SDL_FPoint points[20];
    for (int i = 0; i < segments; ++i) {
        float a = (float) i * (2.f * 3.14159265f / segments);
        points[i].x = cx + SDL_cosf(a) * radius;
        points[i].y = cy + SDL_sinf(a) * radius;
    }
    fill_fan(renderer, points, segments, color);
}

static int
round_rect_points(SDL_FRect rect, float radius, SDL_FPoint *points) {
    if (radius > rect.w / 2.f) {
        radius = rect.w / 2.f;
    }
    if (radius > rect.h / 2.f) {
        radius = rect.h / 2.f;
    }
    if (radius < 0) {
        radius = 0;
    }

    const int segments = 4;
    const float half_pi = 1.5707963f;
    int n = 0;

    // top-right, bottom-right, bottom-left, top-left
    const float corner_x[4] = {
        rect.x + rect.w - radius,
        rect.x + rect.w - radius,
        rect.x + radius,
        rect.x + radius,
    };
    const float corner_y[4] = {
        rect.y + radius,
        rect.y + rect.h - radius,
        rect.y + rect.h - radius,
        rect.y + radius,
    };
    const float start_angle[4] = {
        -half_pi, 0.f, half_pi, 2.f * half_pi,
    };

    for (int corner = 0; corner < 4; ++corner) {
        for (int i = 0; i <= segments; ++i) {
            float a = start_angle[corner]
                    + half_pi * (float) i / (float) segments;
            points[n].x = corner_x[corner] + SDL_cosf(a) * radius;
            points[n].y = corner_y[corner] + SDL_sinf(a) * radius;
            ++n;
        }
    }
    return n;
}

static void
fill_round_rect(SDL_Renderer *renderer, SDL_FRect rect, float radius,
                SDL_Color color) {
    SDL_FPoint points[24];
    int n = round_rect_points(rect, radius, points);
    fill_fan(renderer, points, n, color);
}

static void
stroke_line(SDL_Renderer *renderer, float x1, float y1, float x2, float y2,
            float width, SDL_Color color) {
    float dx = x2 - x1;
    float dy = y2 - y1;
    float len = SDL_sqrtf(dx * dx + dy * dy);
    if (len < 0.001f) {
        return;
    }

    float half = width / 2.f;
    float nx = -dy / len * half;
    float ny = dx / len * half;

    SDL_FPoint quad[4] = {
        { x1 + nx, y1 + ny },
        { x2 + nx, y2 + ny },
        { x2 - nx, y2 - ny },
        { x1 - nx, y1 - ny },
    };
    fill_fan(renderer, quad, 4, color);
    fill_circle(renderer, x1, y1, half, color);
    fill_circle(renderer, x2, y2, half, color);
}

static void
draw_icon(SDL_Renderer *renderer, int icon, float cx, float cy,
          SDL_Color color) {
    switch (icon) {
        case ICON_BACK:
            stroke_line(renderer, cx + 8, cy, cx - 6, cy, ICON_STROKE, color);
            stroke_line(renderer, cx - 6, cy, cx + 2, cy - 8, ICON_STROKE,
                        color);
            stroke_line(renderer, cx - 6, cy, cx + 2, cy + 8, ICON_STROKE,
                        color);
            break;
        case ICON_HOME: {
            SDL_FPoint roof[3] = {
                { cx, cy - 11 },
                { cx + 11, cy - 1 },
                { cx - 11, cy - 1 },
            };
            fill_fan(renderer, roof, 3, color);
            SDL_FRect body = { cx - 7.5f, cy - 1, 15, 11 };
            fill_round_rect(renderer, body, 2.f, color);
            break;
        }
        case ICON_RECENTS: {
            SDL_Color back = { color.r, color.g, color.b, 105 };
            SDL_FRect back_rect = { cx - 10, cy - 5, 13, 15 };
            fill_round_rect(renderer, back_rect, 3.f, back);
            SDL_FRect front_rect = { cx - 3, cy - 10, 13, 15 };
            fill_round_rect(renderer, front_rect, 3.f, color);
            break;
        }
        case ICON_VOLUME_DOWN:
        case ICON_VOLUME_UP: {
            SDL_FPoint speaker[6] = {
                { cx - 12, cy - 3 },
                { cx - 7, cy - 3 },
                { cx - 2, cy - 9 },
                { cx - 2, cy + 9 },
                { cx - 7, cy + 3 },
                { cx - 12, cy + 3 },
            };
            fill_fan(renderer, speaker, 6, color);
            stroke_line(renderer, cx + 2, cy, cx + 10, cy, ICON_STROKE,
                        color);
            if (icon == ICON_VOLUME_UP) {
                stroke_line(renderer, cx + 6, cy - 4, cx + 6, cy + 4,
                            ICON_STROKE, color);
            }
            break;
        }
        case ICON_POWER: {
            const float radius = 9;
            const float start = -0.96f; // about -55 degrees
            const float end = 4.10f;    // about 235 degrees
            const int segments = 16;

            float px = cx + SDL_cosf(start) * radius;
            float py = cy + SDL_sinf(start) * radius;
            for (int i = 1; i <= segments; ++i) {
                float a = start + (end - start) * (float) i / segments;
                float x = cx + SDL_cosf(a) * radius;
                float y = cy + SDL_sinf(a) * radius;
                stroke_line(renderer, px, py, x, y, ICON_STROKE, color);
                px = x;
                py = y;
            }
            stroke_line(renderer, cx, cy - 11, cx, cy - 1, ICON_STROKE, color);
            break;
        }
    }
}

bool
sc_navbar_handle_event(struct sc_navbar *nb, SDL_Window *window,
                       const SDL_Event *event, int *keycode) {
    *keycode = -1;

    int ww;
    int wh;
    if (!SDL_GetWindowSize(window, &ww, &wh)) {
        return false;
    }
    float ui_scale = get_ui_scale(window);
    if (ww <= 0 || wh <= SC_NAVBAR_HEIGHT * ui_scale) {
        return false;
    }

    // Work in window points
    float ww_pt = (float) ww / ui_scale;
    float wh_pt = (float) wh / ui_scale;
    int ww_i = (int) ww_pt;
    int wh_i = (int) wh_pt;

    float bar_y = wh_pt - SC_NAVBAR_HEIGHT;

    switch (event->type) {
        case SDL_EVENT_MOUSE_MOTION: {
            float x = event->motion.x / ui_scale;
            float y = event->motion.y / ui_scale;
            bool inside = y >= bar_y;
            int hover = inside ? get_button_at(ww_i, wh_i, x) : -1;
            bool changed = hover != nb->hover;
            nb->hover = hover;
            // Consume events inside the bar, and the event which clears the
            // hover state when leaving a button
            return inside || changed;
        }
        case SDL_EVENT_MOUSE_BUTTON_DOWN:
        case SDL_EVENT_MOUSE_BUTTON_UP: {
            if (event->button.button != SDL_BUTTON_LEFT) {
                return false;
            }
            float x = event->button.x / ui_scale;
            float y = event->button.y / ui_scale;
            bool inside = y >= bar_y;

            if (event->type == SDL_EVENT_MOUSE_BUTTON_DOWN) {
                if (!inside) {
                    return false;
                }
                nb->pressed = get_button_at(ww_i, wh_i, x);
                return true;
            }

            // Button up
            if (nb->pressed < 0) {
                return inside;
            }
            int pressed = nb->pressed;
            nb->pressed = -1;
            if (inside && get_button_at(ww_i, wh_i, x) == pressed) {
                *keycode = button_keycodes[pressed];
            }
            return true;
        }
        case SDL_EVENT_WINDOW_MOUSE_LEAVE:
            if (nb->hover >= 0) {
                nb->hover = -1;
                return true;
            }
            return false;
        default:
            return false;
    }
}

void
sc_navbar_render(struct sc_navbar *nb, SDL_Renderer *renderer,
                 SDL_Window *window) {
    int ww;
    int wh;
    if (!SDL_GetWindowSize(window, &ww, &wh)) {
        return;
    }
    float ui_scale = get_ui_scale(window);
    if (ww <= 0 || wh <= SC_NAVBAR_HEIGHT * ui_scale) {
        return;
    }

    // Draw in window points (the renderer normally works in pixels)
    SDL_SetRenderScale(renderer, ui_scale, ui_scale);
    SDL_SetRenderDrawBlendMode(renderer, SDL_BLENDMODE_BLEND);

    int ww_pt = (int) ((float) ww / ui_scale);
    int wh_pt = (int) ((float) wh / ui_scale);

    float bar_y = (float) wh_pt - SC_NAVBAR_HEIGHT;

    // Dark translucent background, with rounded top corners
    const float corner = 18.f;
    SDL_Color bg = { 13, 16, 22, 244 };
    SDL_FPoint bar_points[20];
    int n = 0;
    // Top-left corner arc (180 -> 270 degrees)
    for (int i = 0; i <= 5; ++i) {
        float a = 3.14159265f + 1.5707963f * (float) i / 5.f;
        bar_points[n].x = corner + SDL_cosf(a) * corner;
        bar_points[n].y = bar_y + corner + SDL_sinf(a) * corner;
        ++n;
    }
    // Top-right corner arc (270 -> 360 degrees)
    for (int i = 0; i <= 5; ++i) {
        float a = 4.71238898f + 1.5707963f * (float) i / 5.f;
        bar_points[n].x = (float) ww_pt - corner + SDL_cosf(a) * corner;
        bar_points[n].y = bar_y + corner + SDL_sinf(a) * corner;
        ++n;
    }
    // Bottom corners (below the window edge, no rounding needed)
    bar_points[n].x = (float) ww_pt;
    bar_points[n].y = (float) wh_pt + 1;
    ++n;
    bar_points[n].x = 0;
    bar_points[n].y = (float) wh_pt + 1;
    ++n;
    fill_fan(renderer, bar_points, n, bg);

    // Accent line along the top edge
    SDL_SetRenderDrawColor(renderer, 0, 225, 170, 90);
    SDL_RenderLine(renderer, corner, bar_y + 1,
                   (float) ww_pt - corner, bar_y + 1);

    SDL_Color icon_normal = { 232, 238, 247, 255 };
    SDL_Color icon_active = { 0, 225, 170, 255 };
    SDL_Color pill_active = { 0, 225, 170, 26 };
    SDL_Color pill_pressed = { 0, 225, 170, 46 };

    for (int i = 0; i < BTN_COUNT; ++i) {
        SDL_FRect rect;
        get_button_rect(ww_pt, wh_pt, i, &rect);

        bool hovered = (i == nb->hover);
        bool pressed = (i == nb->pressed);
        if (hovered || pressed) {
            SDL_FRect pill = {
                rect.x + 6,
                rect.y + 8,
                rect.w - 12,
                rect.h - 16,
            };
            fill_round_rect(renderer, pill, 12.f,
                            pressed ? pill_pressed : pill_active);
        }

        draw_icon(renderer, i, rect.x + rect.w / 2.f, rect.y + rect.h / 2.f,
                  (hovered || pressed) ? icon_active : icon_normal);
    }

    // Separator between navigation keys and device controls
    SDL_FRect sep_rect;
    get_button_rect(ww_pt, wh_pt, 3, &sep_rect);
    SDL_Color sep = { 255, 255, 255, 22 };
    SDL_FRect separator = {
        sep_rect.x - 1,
        sep_rect.y + 18,
        1,
        sep_rect.h - 36,
    };
    SDL_SetRenderDrawColor(renderer, sep.r, sep.g, sep.b, sep.a);
    SDL_RenderFillRect(renderer, &separator);

    // Restore the default render scale
    SDL_SetRenderScale(renderer, 1, 1);
}
